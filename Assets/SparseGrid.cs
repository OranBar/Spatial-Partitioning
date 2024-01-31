using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SparseGrid<T>
{
    public class SparseGridCell {
        public List<T> elements;
        public Bounds bounds;

        public SparseGridCell(Vector3 center)
        {
            elements = new List<T>();
            bounds = new Bounds(
                center, 
                Vector3.one * grid_cell_size
            );
        }
        
    }


    public Dictionary<Vector3, SparseGridCell> grid;
    private const int grid_cell_size = 1; 
    // private const int grid_cell_size = 10000; 

    public SparseGrid()
    {
        this.grid = new Dictionary<Vector3, SparseGridCell>();
    }

    public void Add(T obj_to_add, Vector3 pos){
        Vector3 obj_grid_cell = GetObj_GridCell(pos);

        if(grid.ContainsKey(obj_grid_cell) == false){
            grid[obj_grid_cell] = new SparseGridCell(pos);
        }

        grid[obj_grid_cell].elements.Add(obj_to_add);
    }
    
    private Vector3 GetObj_GridCell(Vector3 pos){
        return new Vector3(
            (int)(pos.x / grid_cell_size),
            (int)(pos.y / grid_cell_size),
            (int)(pos.z / grid_cell_size)
        );
    }
    
    public void Remove(T obj_to_remove, Vector3 pos){
        Vector3 obj_grid_cell = GetObj_GridCell(pos);

        if(grid[obj_grid_cell] == null){
            throw new System.Exception("Object not in grid");
        }

        grid[obj_grid_cell].elements.Remove(obj_to_remove);
    }

    
}
