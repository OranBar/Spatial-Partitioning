using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace OBLib.QuadTree 
{
    public class QuadTreeElementLocation {

    }

    // public struct Vector2 {
    //     public float X;
    //     public float Y;

    //     public Vector2(float x, float y)
    //     {
    //         this.x = x;
    //         this.y = y;
    //     }
    // }

    public class Square {
        public Vector2 center;
        public float halfExtents;

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

        public bool Intersects(Square search_area)
        {
            return Vector3.Distance(this.center, search_area.center) <= this.halfExtents + search_area.halfExtents;
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

        public QuadTree_TrackedObj(T obj, Vector2 objPos, float objRadius)
        {
            this.value = obj;
            this.position = objPos;
            this.radius = objRadius;
        }
    }


    public class QuadTreeNode<T> {
        public Square area;
        private QuadTreeNode<T>[] subQuads = new QuadTreeNode<T>[]{null, null, null, null};
        private List<QuadTree_TrackedObj<T>> node_elements;
        private int max_node_capacity;
        
        public List<T> All_Elements {
            get{
                List<T> result = new List<T>();
                foreach(var c_elem in node_elements){
                    result.Add(c_elem.value);
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
                return subQuads[3] != null;
            }
        }

        public QuadTreeNode(Square area, int max_node_capacity=10)
        {
            node_elements = new List<QuadTree_TrackedObj<T>>(max_node_capacity);
            this.area = area;
            this.max_node_capacity = max_node_capacity;
        }

        public bool Add(T obj_to_add, Vector2 obj_pos, float obj_radius)
        {
            QuadTree_TrackedObj<T> quadtracked_obj = new QuadTree_TrackedObj<T>(obj_to_add, obj_pos, obj_radius);
            return Add(quadtracked_obj);
        }

        private bool Add(QuadTree_TrackedObj<T> obj)
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
        
        public bool Remove(T obj_to_remove, Vector2 obj_pos, float obj_radius){
            // We need to use the QuadTreeElementLocation to find the list from which we need to remove the element.
            // After removing, the tree might need to be pruned. This can be done now, or be deferred to the end of the frame.

            return false;
        }

        public bool Prune(){

            return false;
        }
        
        public bool Contains(T obj, Vector2 obj_pos, float obj_radius){
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

        public List<T> Search(Square search_area)
        {
            List<T> result = new List<T>();

            foreach(var c_elem in this.node_elements){
                if(search_area.Intersects(c_elem.position, c_elem.radius)){
                    result.Add(c_elem.value);
                }
            }
            
            if(IsSubdivided){
                foreach(var c_subquad in subQuads){
                    if(search_area.Contains(c_subquad.area)){
                        result.AddRange(c_subquad.All_Elements);
                    }

                    if(c_subquad.area.Intersects(search_area)){
                        List<T> recursive_search = c_subquad.Search(search_area); 
                        if(recursive_search.IsNullOrEmpty() == false){
                            result.AddRange(recursive_search);
                        }
                    }

                    // List<T> search_result = c_subquad.Search(search_area);
                    // if(search_result != null && search_result.Count >= 1){
                    //     result.AddRange(search_result);
                    // }
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

            foreach(QuadTreeNode<T> c_subquad in this.subQuads){
                c_subquad.Clear();
            }
        }
    }


    public class QuadTree<T>
    {
        // public static readonly int MAX_NODE_CAPACITY = 10;


        public QuadTreeNode<T> root;
        private int max_node_capacity;
        private int max_depth;
        private List<T> objects;
        public List<T>.Enumerator Objects { 
            get{
                return objects.GetEnumerator();
            }
        }


        public QuadTree(Square bounds, int max_node_capacity, int max_depth)
        {
            this.max_depth = max_depth;
            this.max_node_capacity = max_node_capacity;
            this.root = new QuadTreeNode<T>(bounds, max_node_capacity);
        }



        public void Add(T objToAdd, Vector2 objPos, float objRadius)
        {
            root.Add(objToAdd, objPos, objRadius);
        }

        public void Remove(T objToRemove){

        }

        public void Clear(){
            root.Clear();
        }

        public void Relocate(){

        }

        public List<T> Search(Square search_area){
            return this.root.Search(search_area);
        }
    }
}