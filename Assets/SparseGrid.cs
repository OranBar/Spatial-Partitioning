using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

public interface ISparseGrid_ElementOperations<T>{
    Vector3 GetPosition(T obj);
    Bounds GetBoundingBox(T obj);
}

public class SparseGridCell<T> {
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
    

public class SparseGrid<T>
{
    public Dictionary<Vector3, SparseGridCell<T>> grid;
    private int grid_cell_size;
    private ISparseGrid_ElementOperations<T> element_operations;

    public int cells_checked_iterations;
    public int element_iterations;
    public int intersecting_cells_iterations;
    public int contained_cells_iterations;

    public SparseGrid(int grid_cell_size, ISparseGrid_ElementOperations<T> obj_position_getter)
    {
        this.grid = new Dictionary<Vector3, SparseGridCell<T>>();
        this.grid_cell_size = grid_cell_size;
        this.element_operations = obj_position_getter;
    }

    public SparseGridCell<T> Add(T obj_to_add){
        Vector3 obj_pos = element_operations.GetPosition(obj_to_add);
        Vector3 obj_grid_cell = GetCellKey_ForPosition(obj_pos);

        if(grid.ContainsKey(obj_grid_cell) == false){
            grid[obj_grid_cell] = new SparseGridCell<T>(
                obj_grid_cell * grid_cell_size, 
                grid_cell_size
            );
        }

        grid[obj_grid_cell].elements.Add(obj_to_add);
        return grid[obj_grid_cell];

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


    public SparseGridCell<T> GetGridCell(T obj)
    {
        return GetGridCell(this.element_operations.GetPosition(obj));
    }

    public SparseGridCell<T> GetGridCell(Vector3 pos)
    {
        Vector3 cell_key = GetCellKey_ForPosition(pos);
        SparseGridCell<T> result = null;
        grid.TryGetValue(cell_key, out result);

        return result;
    }
    
    public SparseGridCell<T> Remove(T obj_to_remove){
        Vector3 obj_pos = this.element_operations.GetPosition(obj_to_remove);
        return Remove(obj_to_remove, obj_pos);
    }

    public SparseGridCell<T> Remove(T obj_to_remove, Vector3 pos){
        Vector3 obj_grid_cell = GetCellKey_ForPosition(pos);

        if(grid[obj_grid_cell] == null){
            throw new System.Exception("Object not in grid");
        }

        grid[obj_grid_cell].elements.Remove(obj_to_remove);

        return grid[obj_grid_cell];
    }
    
    // Performance-Upgrade: If we keep a min and a max elmeent that define the max bounds of your grid, we can immediately discard all cells outside our bounds, and drastically reduce search queries for areas that are very large.
    // If we don't, we'll have to check a ton of cells for big queries....  The upside is that they'll be empty
    // One thing we could do is intersect the bounds of all objects with the search area, and use the result when querying the grid cells, to avoid uncecessary calls 
    public List<T> Search(Bounds search_area){
        List<T> result = new List<T>();

        cells_checked_iterations = 0;
        element_iterations = 0;
        intersecting_cells_iterations = 0;
        contained_cells_iterations = 0;


        // List<SparseGridCell> cells_to_check = new List<SparseGridCell>();
        Vector3 min_cell = GetCellKey_ForPosition(search_area.min);
        Vector3 max_cell = GetCellKey_ForPosition(search_area.max);
        for (int x = (int) min_cell.x; x <= (int) max_cell.x; x++){
            bool x_edge = x == min_cell.x || x == max_cell.x;

            for (int y = (int) min_cell.y; y <= (int) max_cell.y; y++){
                bool y_edge = y == min_cell.y || y == max_cell.y;

                for (int z = (int) min_cell.z; z <= (int) max_cell.z; z++){
                    cells_checked_iterations++;

                    Vector3 curr_key = new Vector3(x, y, z); 
                    if(grid.ContainsKey(curr_key)){

                        // I think this condition is true on 1st and last iteration of every loop, and false otherwise
                        bool z_edge = z == min_cell.z || z == max_cell.z;
                        bool cell_intersects_search_area = x_edge || y_edge || z_edge;

                        // if(grid[curr_key].bounds.Intersects(search_area) == false){
                        //     // If we're not intersecting, we're fully contained
                        //     result.AddRange(grid[curr_key].elements);
                        // if (cell_intersects_search_area)
                        if(cell_intersects_search_area)
                        {
                            intersecting_cells_iterations++;
                            // We're intersecting, so we need to check which elements from this grid cell are actually inside the serach area
                            foreach (var c_elem in grid[curr_key].elements)
                            {
                                element_iterations++;
                                Vector3 c_elem_position = element_operations.GetPosition(c_elem);
                                if (search_area.Contains(c_elem_position))
                                {
                                    result.Add(c_elem);
                                }
                            }
                        }
                        else
                        {
                        //  If the cell we are looking at is fully contained in the search_area, we can add all elements of the cells without doing the Contains check
                            contained_cells_iterations++;
                            result.AddRange(grid[curr_key].elements);
                        }
                    }

                }
            }
        }
        return result;
    }

    
}
