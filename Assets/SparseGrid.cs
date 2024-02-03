using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;

public interface ISparseGrid_ElementOperations<T>{
    Vector3 GetPosition(T obj);
    Bounds GetBoundingBox(T obj);
}

public class SparseGrid<T>
{
    public class SparseGridCell {
        public List<T> elements;
        public Bounds bounds;

        public SparseGridCell(Vector3 center, int grid_cell_size)
        {
            elements = new List<T>();
            bounds = new Bounds(
                center, 
                Vector3.one * grid_cell_size
            );
        }
        
    }


    public Dictionary<Vector3, SparseGridCell> grid;
    public int cell_iterations;
    public int element_iterations;
    private int grid_cell_size = 10;
    private ISparseGrid_ElementOperations<T> element_operations;

    public Vector3 min_bounds = Vector3.zero;
    public Vector3 max_bounds = Vector3.zero;

    // private const int grid_cell_size = 1; 
    // private const int grid_cell_size = 10000; 

    public SparseGrid(int grid_cell_size, ISparseGrid_ElementOperations<T> obj_position_getter)
    {
        this.grid = new Dictionary<Vector3, SparseGridCell>();
        this.grid_cell_size = grid_cell_size;
        this.element_operations = obj_position_getter;
    }

    public void Add(T obj_to_add, Vector3 pos){
        Vector3 obj_grid_cell = GetCellKey_ForPosition(pos);

        if(grid.ContainsKey(obj_grid_cell) == false){
            grid[obj_grid_cell] = new SparseGridCell(pos, grid_cell_size);
        }

        grid[obj_grid_cell].elements.Add(obj_to_add);

        // Bounds obj_bounds = this.element_operations.GetBoundingBox(obj_to_add);
        
        // this.min_bounds.x = Mathf.Min(obj_bounds.min.x, this.min_bounds.x);
        // this.min_bounds.y = Mathf.Min(obj_bounds.min.y, this.min_bounds.y);
        // this.min_bounds.z = Mathf.Min(obj_bounds.min.z, this.min_bounds.z);
        
        // this.max_bounds.x = Mathf.Max(obj_bounds.max.x, this.max_bounds.x);
        // this.max_bounds.y = Mathf.Max(obj_bounds.max.y, this.max_bounds.y);
        // this.max_bounds.z = Mathf.Max(obj_bounds.max.z, this.max_bounds.z);
        
        // Goddamit, this is easy, but when removing, we'll have a super tough time updating this number!
    }
    
    private Vector3 GetCellKey_ForPosition(Vector3 pos){
        return new Vector3(
            (int)(pos.x / grid_cell_size),
            (int)(pos.y / grid_cell_size),
            (int)(pos.z / grid_cell_size)
        );
    }


    private SparseGridCell GetGridCell(Vector3 pos)
    {
        Vector3 cell_key = GetCellKey_ForPosition(pos);
        return grid[cell_key];
    }
    
    public void Remove(T obj_to_remove, Vector3 pos){
        Vector3 obj_grid_cell = GetCellKey_ForPosition(pos);

        if(grid[obj_grid_cell] == null){
            throw new System.Exception("Object not in grid");
        }

        grid[obj_grid_cell].elements.Remove(obj_to_remove);
    }
    
    // Performance-Upgrade: If we keep a min and a max elmeent that define the max bounds of your grid, we can immediately discard all cells outside our bounds, and drastically reduce search queries for areas that are very large.
    // If we don't, we'll have to check a ton of cells for big queries....  The upside is that they'll be empty
    // One thing we could do is intersect the bounds of all objects with the search area, and use the result when querying the grid cells, to avoid uncecessary calls 
    public List<T> Search(Bounds search_area){
        List<T> result = new List<T>();

        cell_iterations = 0;
        element_iterations = 0;

        // List<SparseGridCell> cells_to_check = new List<SparseGridCell>();
        Vector3 min_cell = GetCellKey_ForPosition(search_area.min);
        Vector3 max_cell = GetCellKey_ForPosition(search_area.max);
        for (int x = (int) min_cell.x; x <= (int) max_cell.x; x++){
            for (int y = (int) min_cell.y; y <= (int) max_cell.y; y++){
                for (int z = (int) min_cell.z; z <= (int) max_cell.z; z++){
                    cell_iterations++;

                    Vector3 curr_key = new Vector3(x, y, z); 
                    if(grid.ContainsKey(curr_key)){
                        // TODO: If the cell we are looking at is fully contained in the search_area, we can add all elements of the cells without doing the Contains check

                        foreach(var c_elem in grid[curr_key].elements){
                            element_iterations++;
                            Vector3 c_elem_position = element_operations.GetPosition(c_elem);
                            if(search_area.Contains( c_elem_position )){
                                result.Add(c_elem);
                            }
                        }
                    }

                }
            }
        }
        return result;
    }

    
}
