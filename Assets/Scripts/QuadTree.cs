using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

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
    
    public Rect(Vector2 top, Vector2 bottom){

        // I'm assuming all Quads are squares
        
        this.halfExtents = Mathf.Abs(top.X - bottom.X) / 2;
        this.center = new Vector2((top.X + bottom.X)/2, (top.Y + bottom.Y)/2);
    }

    // Only returns true if the object is fully contained in the rect
    public bool Contains(Vector2 center, float radius){

        Vector2[] corners = GetCorners();
        return (
            center.X - radius >= corners[0].X && center.X + radius <= corners[1].X &&
            center.Y - radius >= corners[2].Y && center.Y + radius <= corners[0].Y
        );
    }

    public Vector2[] GetCorners(){
        Vector2[] result = new Vector2[4];
        
        // top_left
        result[0] = new Vector2(this.center.X - this.halfExtents, this.center.Y + halfExtents);
            // top_right
        result[1] = new Vector2(this.center.X + this.halfExtents, this.center.Y + halfExtents);
        // bottom_left
        result[2] = new Vector2(this.center.X + this.halfExtents, this.center.Y - halfExtents);
        // bottom_right
        result[3] = new Vector2(this.center.X - this.halfExtents, this.center.Y - halfExtents);

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
    public Rect area;
    private QuadTreeNode<T>[] subQuads = new QuadTreeNode<T>[]{null, null, null, null};
    private List<QuadTree_TrackedObj<T>> elements;
    private int max_node_capacity;

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
    public List<QuadTree_TrackedObj<T>>.Enumerator Elements {
        get{
            return elements.GetEnumerator();
        }
    }

    public bool IsSubdivided { 
        get{
            return subQuads[3] != null;
        }
    }

    public QuadTreeNode(Rect area, int max_node_capacity=10)
    {
        elements = new List<QuadTree_TrackedObj<T>>(max_node_capacity);
        this.area = area;
        this.max_node_capacity = max_node_capacity;
    }

    public bool Add(T objToAdd, Vector2 objPos, float objRadius)
    {
        // Redundant check, but it might allows us to skip allocating the QuadTree_TrackedObj when unecessary.
        if (this.area.Contains(objPos, objRadius) == false)
        {
            return false;
        }

        QuadTree_TrackedObj<T> quadtracked_obj = new QuadTree_TrackedObj<T>(objToAdd, objPos, objRadius);
        return Add(quadtracked_obj);
    }

    public bool Add(QuadTree_TrackedObj<T> obj)
    {
        if (this.area.Contains(obj.position, obj.radius) == false)
        {
            return false;
        }

        // We might need to subdivide if this item fills our leaf to max capacity
        if (IsSubdivided == false)
        {
            if(elements.Count + 1 > max_node_capacity){
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

        elements.Add(obj);
        // If we're here, the object overlaps at least two of the sub quads.
        return true;
        

        // if (elements.Count + 1 <= max_node_capacity)
        // {
        //     elements.Add(obj);
        //     return true;
        // }
        // else
        // {
        //     if (IsSubdivided == false)
        //     {
        //         Subdivide();
        //     }

        //     if (TopLeft.Add(obj)) { return true; }
        //     if (TopRight.Add(obj)) { return true; }
        //     if (BottomLeft.Add(obj)) { return true; }
        //     if (BottomRight.Add(obj)) { return true; }

        //     // If we're here, the object overlaps at least two of the sub quads.
        //     elements.Add(obj);
        //     return true;
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

        subQuads[0] = new QuadTreeNode<T>(top_left, max_node_capacity);
        subQuads[1] = new QuadTreeNode<T>(top_right, max_node_capacity);
        subQuads[2] = new QuadTreeNode<T>(bottom_left, max_node_capacity);
        subQuads[3] = new QuadTreeNode<T>(bottom_right, max_node_capacity);
        
        for (int i = this.elements.Count - 1; i >= 0 ; i--)
        {
            var c_elem = this.elements[i];
            foreach(var c_subquad in this.subQuads){
                if(c_subquad.Add(c_elem)){
                    this.elements.RemoveAt(i);
                    continue;
                }
            }
        }

        // var top_left = new BoundingBox(this.area.Center - this.area.HalfExtents, this.area.Center);
        // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
        // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
        // var top_right = new BoundingBox(this.area.Center + new Vector3(this.area.HalfExtents.X, 0, this.area.HalfExtents.Z), this.area.Center);
        // var bottom_right = new BoundingBox(this.area.Center + this.area.HalfExtents, this.area.Center);
        
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
    
    
    public QuadTree(Rect bounds, int max_node_capacity, int max_depth)
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
        
    }
    
    public void Relocate(){
        
    }
    
    public List<T> Search(Rect box){
        return null;
    }
    
    
}
