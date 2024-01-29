using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using NaughtyAttributes;

namespace OBLib.QuadTree 
{
    // If we save this pointer both in the quadtree and the list, then when this becomes null, it will become null in both containers. So I can remove by setting to null in the quadtree, and when I'll iterate, I can remove the nulls and prune/reshape the tree
    public class QuadTreeElementLocation {

    }

    public class Square {
        public Vector2 center;
        public float halfExtents;

        public float X_min => center.x - halfExtents;
        public float X_max => center.x + halfExtents;
        public float Y_min => center.y - halfExtents;
        public float Y_max => center.y + halfExtents;

        public Vector2 TopLeft {
            get{
                return new Vector2(this.center.x - this.halfExtents, this.center.y + halfExtents);
            }
        } 
        public Vector2 TopRight
        {
            get{
                return new Vector2(this.center.x + this.halfExtents, this.center.y + halfExtents);
            }
        } 

        public Vector2 BottomLeft
        {
            get{
                return new Vector2(this.center.x + this.halfExtents, this.center.y - halfExtents);
            }
        } 

        public Vector2 BottomRight
        {
            get{
                return new Vector2(this.center.x - this.halfExtents, this.center.y - halfExtents);
            }
        }


        public Square( Vector2 center, float halfExtents )
        {
            this.center = center;
            this.halfExtents = halfExtents;
        }

        public Square(Vector2 top, Vector2 bottom){

            // I'm assuming all Quads are squares
            this.halfExtents = Mathf.Abs(top.x - bottom.x) / 2;
            this.center = new Vector2((top.x + bottom.x)/2, (top.y + bottom.y)/2);
        }

        // Only returns true if the object is fully contained in the rect
        public bool Contains(Vector2 center, float radius){

            Vector2[] corners = GetCorners();
            return (
                    center.x - radius >= corners[0].x && center.x + radius <= corners[1].x &&
                    center.y - radius >= corners[2].y && center.y + radius <= corners[0].y
                   );
        }
        
        public bool Intersects(Vector2 center, float radius){

            Vector2[] corners = GetCorners();
            return (
                    center.x + radius >= corners[0].x && center.x - radius <= corners[1].x &&
                    center.y + radius >= corners[2].y && center.y - radius <= corners[0].y
                   );
        }

        public bool Intersects(Square other)
        {
            //TODO: I think this doesn't work
            return other.X_max > this.X_min && other.X_min < this.X_max &&
                other.Y_max > this.Y_min && other.Y_min < this.Y_max;
        }

        public bool Contains(Square search_area)
        {
            bool x_contained = search_area.TopRight.x < this.TopRight.x && search_area.TopLeft.x > this.TopLeft.x;
            
            bool y_contained = search_area.TopRight.y < this.TopRight.y && search_area.BottomRight.y > this.BottomRight.y;

            return x_contained && y_contained;
        }

        public Vector2[] GetCorners(){
            Vector2[] result = new Vector2[4];

            result[0] = TopLeft;
            result[1] = TopRight;
            result[2] = BottomLeft;
            result[3] = BottomRight;

            return result;
        }

    }

    // I really want to make it a struct, but I want to handle this data with pointers, so I'm going with class.
    public class QuadTree_TrackedObj<T>{
        public T value;
        public Vector2 position;
        public float radius;
        public bool was_destroyed;

        public QuadTree_TrackedObj(T obj, Vector2 objPos, float objRadius)
        {
            this.value = obj;
            this.position = objPos;
            this.radius = objRadius;
            this.was_destroyed = false;
        }
    }


    public class QuadTreeNode<T> {
        public Square area;
        private QuadTreeNode<T>[] subQuads = new QuadTreeNode<T>[]{null, null, null, null};
        private List<QuadTree_TrackedObj<T>> node_elements;
        // TODO: The nodes all save a max_capacity variable. It's a waste of space
        private int max_node_capacity;
        
        public List<QuadTree_TrackedObj<T>> All_Elements {
            get{
                List<QuadTree_TrackedObj<T>> result = new List<QuadTree_TrackedObj<T>>();
                foreach(var c_elem in node_elements){
                    result.Add(c_elem);
                }

                if(IsSubdivided){
                    foreach(var c_subquad in subQuads){
                        result.AddRange(c_subquad.All_Elements);
                    }
                }

                return result;
            }
        }

        public QuadTreeNode<T> TopLeft {
            get{
                return subQuads[0];
            }
        } 
        public QuadTreeNode<T> TopRight
        {
            get{
                return subQuads[1];
            }
        } 

        public QuadTreeNode<T> BottomLeft
        {
            get{
                return subQuads[2];
            }
        } 

        public QuadTreeNode<T> BottomRight
        {
            get{
                return subQuads[3];
            }
        }

        // This list is not always bounded. If there are many overlap elements, or if the max depth of the tree has been reached, we don't know in advance how many objects we'll need to store.
        public List<QuadTree_TrackedObj<T>>.Enumerator Node_Elements {
            get{
                return node_elements.GetEnumerator();
            }
        }

        public bool IsSubdivided { 
            get{
                // It should never be the case that one subquad is null, while the other are not null. So I can get away with checking just 1 out of the 4 subquads. 
                return subQuads[3] != null;
            }
        }

        public QuadTreeNode(Square area, int max_node_capacity=10)
        {
            node_elements = new List<QuadTree_TrackedObj<T>>(max_node_capacity);
            this.area = area;
            this.max_node_capacity = max_node_capacity;
        }

        // public bool Add(T obj_to_add, Vector2 obj_pos, float obj_radius)
        // {
        //     QuadTree_TrackedObj<T> quadtracked_obj = new QuadTree_TrackedObj<T>(obj_to_add, obj_pos, obj_radius);
        //     return Add(quadtracked_obj);
        // }

        public bool Add(QuadTree_TrackedObj<T> obj)
        {
            if (this.area.Contains(obj.position, obj.radius) == false)
            {
                return false;
            }

            // We might need to subdivide if this item fills our leaf to max capacity
            if (IsSubdivided == false)
            {
                if(node_elements.Count + 1 > max_node_capacity){
                    Subdivide();
                }
            }

            // When subdivided, we add the elem to every sub quad. 
            // If the element is not completely contained in a sub quad, 
            // the Add will return false, and the element will be appended to 
            // the elements list of the current node
            if(IsSubdivided){
                if (TopLeft.Add(obj)) { return true; }
                if (TopRight.Add(obj)) { return true; }
                if (BottomLeft.Add(obj)) { return true; }
                if (BottomRight.Add(obj)) { return true; }
            }

            node_elements.Add(obj);
            // If we're here, the object overlaps at least two of the sub quads.
            return true;

        }
        
        public QuadTree_TrackedObj<T> Remove(T obj_to_remove, Vector2 obj_pos, float obj_radius){
            // We need to use the QuadTreeElementLocation to find the list from which we need to remove the element.
            // After removing, the tree might need to be pruned. This can be done now, or be deferred to the end of the frame.

            for (int i = node_elements.Count - 1; i >= 0; i--)
            {
                QuadTree_TrackedObj<T> c_elem = this.node_elements[i];
                if (c_elem.value.Equals(obj_to_remove)){
                    var removed_elem = node_elements[i];
                    node_elements.RemoveAt(i);

                    return removed_elem;
                }
            }
            
            if(IsSubdivided){
                foreach(var c_subquad in subQuads)
                {
                    if(c_subquad.Contains(obj_pos, obj_radius)){
                        return c_subquad.Remove(obj_to_remove, obj_pos, obj_radius);
                    }
                }
            }
            
            return null;
        }

        // TODO: Honestly I doubt it's worth it to prune. We'll have to iterate all quads in the tree all the way down and back up again. My guess is that remaking the from scratch tree is faster, altho I could have more problems with allocations if I do that. 
        public bool Prune(){
            bool can_prune_node = true;
            if(TopLeft != null && TopLeft.node_elements.Count > 0){
                can_prune_node = can_prune_node && TopLeft.Prune();
                // can_prune_node = false;
            }
            if(TopRight != null && TopRight.node_elements.Count > 0){
                can_prune_node = can_prune_node && TopRight.Prune();
                // can_prune_node = false;
            }
            if(BottomLeft != null && BottomLeft.node_elements.Count > 0){
                can_prune_node = can_prune_node && BottomLeft.Prune();
                // can_prune_node = false;
            }
            if(BottomRight != null && BottomRight.node_elements.Count > 0){
                can_prune_node = can_prune_node && BottomRight.Prune(); 
                // can_prune_node = false;
            }

            if(can_prune_node){
                this.Clear();
                return true;
            }
            
            return false;
        }
        
        public bool Contains(Vector2 obj_pos, float obj_radius){
            return this.area.Contains(obj_pos, obj_radius);
        }

        public bool Contains(Square search_area)
        {
            return this.area.Contains(search_area);
        }

        public void Subdivide(){
            Vector2[] corners = this.area.GetCorners();

            var top_left     = new Square( corners[0], this.area.center);
            var top_right    = new Square( corners[1], this.area.center);
            var bottom_left  = new Square( corners[2], this.area.center);

            var bottom_right = new Square( corners[3], this.area.center);

            subQuads[0] = new QuadTreeNode<T>(top_left,     max_node_capacity);
            subQuads[1] = new QuadTreeNode<T>(top_right,    max_node_capacity);
            subQuads[2] = new QuadTreeNode<T>(bottom_left,  max_node_capacity);
            subQuads[3] = new QuadTreeNode<T>(bottom_right, max_node_capacity);

            for (int i = this.node_elements.Count - 1; i >= 0 ; i--)
            {
                var c_elem = this.node_elements[i];
                foreach(var c_subquad in this.subQuads){
                    if(c_subquad.Add(c_elem)){
                        this.node_elements.RemoveAt(i);
                        // TODO: this continue should continue the outer for, not the inner
                        continue;
                    }
                }
            }

        }

        public int search__iterations = 0;

        public List<QuadTree_TrackedObj<T>> Search(Square search_area)
        {
            List<QuadTree_TrackedObj<T>> result = new List<QuadTree_TrackedObj<T>>();

            foreach(var c_elem in this.node_elements){
                search__iterations++;
                if(search_area.Intersects(c_elem.position, c_elem.radius)){
                    result.Add(c_elem);
                }
            }
            
            if(IsSubdivided){
                foreach(var c_subquad in subQuads)
                {
                    search__iterations++;
                    if(search_area.Contains(c_subquad.area))
                    {
                        result.AddRange(c_subquad.All_Elements);
                    } 
                    else if(search_area.Intersects(c_subquad.area))
                    {
                        List<QuadTree_TrackedObj<T>> recursive_search = c_subquad.Search(search_area); 
                        if (recursive_search != null && recursive_search.Count > 0){
                            result.AddRange(recursive_search);
                        }
                    }
                }
            }

            // If we return a non-null list, it better have at least one element or I'll get mad
            if(result.Count == 0){
                return null;
            }

            return result;
        }

        public void Clear(){
            this.node_elements.Clear();

            for (int i = 0; i < subQuads.Length; i++)
            {
                QuadTreeNode<T> c_subquad = this.subQuads[i];
                c_subquad.Clear();
                this.subQuads[i] = null;
            }
        }
    }


    public class QuadTree<T>
    {
        // public static readonly int MAX_NODE_CAPACITY = 10;

        public QuadTreeNode<T> root;
        // If we don't put a max_depth, the default behaviour is: No cell will be subdivided if it is smaller than the smallest element. No element will be placed in a cell that doesn't fully contain him. The lower bounds for the cell size is at least as big as the smallest object inserted in the quadtree.
        private int max_depth;
        private List<QuadTree_TrackedObj<T>> scene_objects;

        public IEnumerable<T> Get_All_Scene_Objects() {
            for (int i = scene_objects.Count - 1; i >= 0; i--)
            {
                var curr_obj = scene_objects[i];
                if(curr_obj.was_destroyed == false){
                    yield return curr_obj.value;
                } else {
                    // Ensure the scene_objects list is synched with the quadtree. Objects with the was_destroy bool set to true have been removed from the quadtree, while the removal from the scene_objects list was deferred until now
                    scene_objects.RemoveAt(i);
                }
            }
        }


        public QuadTree(Square bounds, int max_node_capacity, int max_depth)
        {
            this.max_depth = max_depth;
            // this.max_node_capacity = max_node_capacity;
            this.root = new QuadTreeNode<T>(bounds, max_node_capacity);
        }



        public void Add(T obj_to_add, Vector2 obj_pos, float obj_radius)
        {
            QuadTree_TrackedObj<T> quadtracked_obj = new QuadTree_TrackedObj<T>(obj_to_add, obj_pos, obj_radius);
            root.Add(quadtracked_obj);
            scene_objects.Add(quadtracked_obj);
        }

        public void Remove(T obj_to_remove, Vector2 obj_pos, float obj_radius){

            QuadTree_TrackedObj<T> removed_obj = root.Remove(obj_to_remove, obj_pos, obj_radius);
            // List<QuadTree_TrackedObj<T>> quadtracked_objs = root.Search(search_area);

            // When we loop over this object by iterating the scene_objects list, we will check this flag to decide if we need to remove it.
            // This syncronizes the two data structures so they both ultimately contain the same objects
            removed_obj.was_destroyed = true;
        }

        public void Clear(){
            root.Clear();
        }

        public void Relocate(T obj_to_relocate, Vector2 obj_prev_position, float obj_radius, Vector2 obj_next_position){
            // First of all, remove
            QuadTree_TrackedObj<T> elem_to_relocate = this.root.Remove(obj_to_relocate, obj_prev_position, obj_radius);
            // Then, reinsert
            if(elem_to_relocate == null){
                throw new Exception("Element Not Found");
            }
            
            elem_to_relocate.position = obj_next_position;
            elem_to_relocate.was_destroyed = false; //This was set to true by the remove method. Flick it back
            this.root.Add(elem_to_relocate);
        }

        public IEnumerable<T> Search(Square search_area){
            this.root.search__iterations = 0;

            var result = this.root.Search(search_area);
            
            return result?.Select(elem => elem.value);
        }
    }
}