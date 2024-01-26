using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        public QuadTreeNode[] SubQuads = new QuadTreeNode[]{null, null, null, null};
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
        private List<T> elements;
        public List<T>.Enumerator Elements {
            get{
                return elements.GetEnumerator();
            }
        }
        
        public QuadTreeNode(Rect area, int max_node_capacity=10)
        {
            elements = new List<T>(max_node_capacity);
            this.area = area;
        }
        
        // public void Subdivide(){
        //     var top_left     = new BoundingBox( new Vector3(this.area.Min.X, 0, this.area.Max.Z), this.area.Center );
        //     var top_right    = new BoundingBox( new Vector3(this.area.Max.X, 0, this.area.Max.Z), this.area.Center );
        //     var bottom_left  = new BoundingBox( new Vector3(this.area.Max.X, 0, this.area.Min.Z), this.area.Center );
        //     var bottom_right = new BoundingBox( new Vector3(this.area.Min.X, 0, this.area.Min.Z), this.area.Center );

        //     SubQuads[0] = new QuadTreeNode(top_left);
        //     SubQuads[1] = new QuadTreeNode(top_right);
        //     SubQuads[2] = new QuadTreeNode(bottom_left);
        //     SubQuads[3] = new QuadTreeNode(bottom_right);

        //     // var top_left = new BoundingBox(this.area.Center - this.area.HalfExtents, this.area.Center);
        //     // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
        //     // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
        //     // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
        //     // var bottom_right = new BoundingBox(this.area.Center + this.area.HalfExtents, this.area.Center);
            
        // }
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
