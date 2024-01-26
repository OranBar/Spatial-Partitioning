using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

public class QuadTreeItemLocation {

}

public struct Vector2 {
    public float X;
    public float Y;

    public Vector2(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }
}

public class Rect {
    public Vector2 center;
    public float halfExtents;

    public Rect( Vector2 center, float halfExtents )
    {
        this.center = center;
        this.halfExtents = halfExtents;
    }
    
    public Rect(Vector2 top_right, Vector2 bottom_left){

        this.halfExtents = top_right.X - bottom_left.X;
        this.center = new Vector2(top_right.X + this.halfExtents, top_right.Y - this.halfExtents);

    }



    // Only returns true if the object is fully contained in the rect
    public bool Contains(Vector2 center, float radius){

        Vector2[] corners = GetCorners();
        return (
            center.X - radius >= corners[0].X && center.X + radius <= corners[1].X &&
            center.Y + radius >= corners[2].Y && center.Y - radius <= corners[3].Y
        );
    }

    public Vector2[] GetCorners(){
        Vector2[] result = new Vector2[4];
        
        // top_left
        result[0] = new Vector2(this.center.X - this.halfExtents, this.center.Y + halfExtents);
            // top_right
        result[1] = new Vector2(this.center.X + this.halfExtents, this.center.Y + halfExtents);
        // bottom_left
        result[2] = new Vector2(this.center.X - this.halfExtents, this.center.Y - halfExtents);
        // bottom_right
        result[3] = new Vector2(this.center.X + this.halfExtents, this.center.Y - halfExtents);

        return result;
    }
}

public class QuadTree<T>
{
    // public static readonly int MAX_NODE_CAPACITY = 10;

    private class QuadTreeNode {
        public Rect area;
        private QuadTreeNode[] SubQuads = new QuadTreeNode[]{null, null, null, null};
        private List<T> elements;
        private int max_node_capacity;

        public QuadTreeNode TopLeft {
            get{
                return SubQuads[0];
            }
        } 
        public QuadTreeNode TopRight
        {
            get{
                return SubQuads[1];
            }
        } 

        public QuadTreeNode BottomLeft
        {
            get{
                return SubQuads[2];
            }
        } 

        public QuadTreeNode BottomRight
        {
            get{
                return SubQuads[3];
            }
        }

        // This list is not always bounded. If there are many overlap elements, or if the max depth of the tree has been reached, we don't know in advance how many objects we'll need to store.
        public List<T>.Enumerator Elements {
            get{
                return elements.GetEnumerator();
            }
        }

        public bool IsSubdivided { 
            get{
                return SubQuads[3] != null;
            }
        }

        public QuadTreeNode(Rect area, int max_node_capacity=10)
        {
            elements = new List<T>(max_node_capacity);
            this.area = area;
            this.max_node_capacity = max_node_capacity;
        }

        public bool Add(T objToAdd, Vector2 objPos, float objRadius){
             
            if(this.area.Contains(objPos, objRadius)){
                if(elements.Count + 1 <= this.max_node_capacity){
                    elements.Add(objToAdd);
                } else {
                    if(this.IsSubdivided == false){
                        Subdivide();
                    }

                    if(TopLeft.Add(objToAdd, objPos, objRadius)) { return true; }
                    if(TopRight.Add(objToAdd, objPos, objRadius)) { return true; }
                    if(BottomLeft.Add(objToAdd, objPos, objRadius)) { return true; }
                    if(BottomRight.Add(objToAdd, objPos, objRadius)) { return true; }

                    // If we're here, the object overlaps at least two of the sub quads.
                    this.elements.Add(objToAdd);
                    return true;
                }
            }

            return false;
            
            // if(this.area.Contains(objPos, objRadius) == false){
            //     // The objects does not fit in this quadrant
            // }
        }
        
        public void Subdivide(){
            Vector2[] corners = this.area.GetCorners();

            var top_left     = new Rect( corners[0], this.area.center);
            var top_right    = new Rect( corners[1], this.area.center);
            var bottom_left  = new Rect( corners[2], this.area.center);
            var bottom_right = new Rect( corners[3], this.area.center);
            
            // var top_right    = new Rect( new Vector2(this.area.Max.X, this.area.Max.Z), this.area.Center );
            // var bottom_left  = new Rect( new Vector2(this.area.Max.X, this.area.Min.Z), this.area.Center );
            // var bottom_right = new Rect( new Vector2(this.area.Min.X, this.area.Min.Z), this.area.Center );

            SubQuads[0] = new QuadTreeNode(top_left);
            SubQuads[1] = new QuadTreeNode(top_right);
            SubQuads[2] = new QuadTreeNode(bottom_left);
            SubQuads[3] = new QuadTreeNode(bottom_right);

            // var top_left = new BoundingBox(this.area.Center - this.area.HalfExtents, this.area.Center);
            // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
            // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
            // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
            // var bottom_right = new BoundingBox(this.area.Center + this.area.HalfExtents, this.area.Center);
            
        }
    }


    private QuadTreeNode root;
    private int max_node_capacity;
    private int max_depth;
    private List<T> objects;
    public List<T>.Enumerator Objects { 
        get{
            return objects.GetEnumerator();
        }
    }
    
    
    public QuadTree(Rect bounds, int max_node_capacity, int max_depth)
    {
        this.max_depth = max_depth;
        this.max_node_capacity = max_node_capacity;
        this.root = new QuadTreeNode(bounds, max_node_capacity);
    }

    

    public void Add(T objToAdd)
    {
        // root.Add(objToAdd);
    }
    
    public void Remove(T objToRemove){
        
    }
    
    public void Clear(){
        
    }
    
    public void Relocate(){
        
    }
    
    public List<T> Search(Rect box){
        return null;
    }
    
    
}
