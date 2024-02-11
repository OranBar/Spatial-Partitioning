using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;

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
    
    // public bool Search(Bounds search_area, List<T> result){
    //     bool z_edge = z == min_cell.z || z == max_cell.z;
    //     bool cell_intersects_search_area = x_edge || y_edge || z_edge;

    //     // if(grid[curr_key].bounds.Intersects(search_area) == false){
    //     //     // If we're not intersecting, we're fully contained
    //     //     result.AddRange(grid[curr_key].elements);
    //     // if (cell_intersects_search_area)
    //     if(cell_intersects_search_area)
    //     {
    //         intersecting_cells_iterations++;
    //         // We're intersecting, so we need to check which elements from this grid cell are actually inside the serach area
    //         foreach (var c_elem in grid[curr_key].elements)
    //         {
    //             element_iterations++;
    //             Vector3 c_elem_position = element_operations.GetPosition(c_elem);
    //             if (search_area.Contains(c_elem_position))
    //             {
    //                 result.Add(c_elem);
    //             }
    //         }
    //     }
    //     else
    //     {
    //     //  If the cell we are looking at is fully contained in the search_area, we can add all elements of the cells without doing the Contains check
    //         contained_cells_iterations++;
    //         result.AddRange(grid[curr_key].elements);
    //     }
    //     return false;
    // }
    
}

public class SparseGrid<T>
{
    public Dictionary<Vector3, SparseGridCell<T>> grid;
    private int grid_cell_size;
    private float half_grid_cell_size;
    private ISparseGrid_ElementOperations<T> element_operations;

    public int cells_checked_iterations;
    public int element_iterations;
    public int intersecting_cells_iterations;
    public int contained_cells_iterations;

    public SparseGrid(int grid_cell_size, ISparseGrid_ElementOperations<T> obj_position_getter)
    {
        this.grid = new Dictionary<Vector3, SparseGridCell<T>>();
        this.grid_cell_size = grid_cell_size;
        this.half_grid_cell_size = grid_cell_size * 0.5f;
        this.element_operations = obj_position_getter;
    }

    private List<SparseGridCell<T>> add_result = new List<SparseGridCell<T>>();

    public List<SparseGridCell<T>> Add(T obj_to_add){
        add_result.Clear();
        Bounds obj_bounds = this.element_operations.GetBoundingBox(obj_to_add);

        HashSet<SparseGridCell<T>> result = new HashSet<SparseGridCell<T>>();
        foreach(var c_cell_params in Get_Cells_In_Area(obj_bounds)){
            if (grid.ContainsKey(c_cell_params.cell_key) == false)
            {
                Vector3 center = c_cell_params.cell_key * grid_cell_size;
                
                grid[c_cell_params.cell_key] = new SparseGridCell<T>( center, grid_cell_size );
            }
            
            grid[c_cell_params.cell_key].elements.Add(obj_to_add);
            result.Add(grid[c_cell_params.cell_key]);
        }

        //TODO: Yeah, tolist is bad, but I do expect this list to never be more than 8 elements, 1 on average, so it's fine
        return result.ToList();
        // Vector3 obj_pos = element_operations.GetPosition(obj_to_add);
        // Vector3 obj_grid_cell = GetCellKey_ForPosition(obj_pos);


        // if(grid.ContainsKey(obj_grid_cell) == false){
            // Vector3 center = obj_grid_cell * grid_cell_size; 
            // if(center.x > 0){
            //     center.x = center.x - half_grid_cell_size;
            // } else if(center.x < 0){
            //     center.x = center.x + half_grid_cell_size;
            // }
            // if(center.y > 0){
            //     center.y = center.y - half_grid_cell_size;
            // } else if(center.y < 0){
            //     center.y = center.y + half_grid_cell_size;
            // }
            
            // if(center.z > 0){
            //     center.z = center.z - half_grid_cell_size;
            // } else if(center.z < 0){
            //     center.z = center.z + half_grid_cell_size;
            // }
            // new Vector3(grid_cell_size, grid_cell_size, grid_cell_size);

            // grid[obj_grid_cell] = new SparseGridCell<T>( center, grid_cell_size );
        // }

        // grid[obj_grid_cell].elements.Add(obj_to_add);
        // return grid[obj_grid_cell];

        // Bounds obj_bounds = this.element_operations.GetBoundingBox(obj_to_add);
        
        // this.min_bounds.x = Mathf.Min(obj_bounds.min.x, this.min_bounds.x);
        // this.min_bounds.y = Mathf.Min(obj_bounds.min.y, this.min_bounds.y);
        // this.min_bounds.z = Mathf.Min(obj_bounds.min.z, this.min_bounds.z);
        
        // this.max_bounds.x = Mathf.Max(obj_bounds.max.x, this.max_bounds.x);
        // this.max_bounds.y = Mathf.Max(obj_bounds.max.y, this.max_bounds.y);
        // this.max_bounds.z = Mathf.Max(obj_bounds.max.z, this.max_bounds.z);
        
        // Goddamit, this is easy, but when removing, we'll have a super tough time updating this number!
    }
    
    // 2 = -2 => 0  
    // 9 = -9 => 0
    // (-10, 10) => 0

    // 12 != -12 => 1(!= -1)  
    // 39 = -39 => 3(!= -3)

    // 2 - 5 = -2 - 5 => 0  
    // 9 = -9 => 0
    // (-10, 10) => 0

    private Vector3 GetCellKey_ForPosition(Vector3 pos){
        int x;
        if(pos.x > 0){
            x = (int) (pos.x + half_grid_cell_size) / grid_cell_size;
        } else if(pos.x < 0){
            x = (int) (pos.x - half_grid_cell_size) / grid_cell_size;
        } else {
            x = 0;
        }

        int y;
        if(pos.y > 0){
            y = (int) (pos.y + half_grid_cell_size) / grid_cell_size;
        } else if(pos.y < 0){
            y = (int) (pos.y - half_grid_cell_size) / grid_cell_size;
        } else {
            y = 0;
        }

        int z;
        if(pos.z > 0){
            z = (int) (pos.z + half_grid_cell_size) / grid_cell_size;
        } else if(pos.z < 0){
            z = (int) (pos.z - half_grid_cell_size) / grid_cell_size;
        } else {
            z = 0;
        }
        
        return new Vector3(x, y, z);
    }

    // TODO: The reason this is broken, is because zero doesn't have a positive and/or a negative, while all other numbers do. This breaks my formula logic for getting positions when the result is 0
    // private Vector3 GetCellKey_ForPosition(Vector3 pos){
    //     return new Vector3(
    //         (int)((pos.x - half_grid_cell_size) / grid_cell_size),
    //         (int)((pos.y - half_grid_cell_size) / grid_cell_size),
    //         (int)((pos.z - half_grid_cell_size) / grid_cell_size)
    //     );
    // }

    // private Vector3 GetCellKey_ForPosition(Vector3 pos){
    //     float half_grid_size = grid_cell_size * 0.5f;
    //     return new Vector3(
    //         (int)((pos.x - half_grid_size) / grid_cell_size),
    //         (int)((pos.y - half_grid_size) / grid_cell_size),
    //         (int)((pos.z - half_grid_size) / grid_cell_size)
    //     );
    // }


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
    
    public struct SparseGridIterationParams {
        public Vector3 cell_key;
        public bool cell_intersects_search_area;

        public SparseGridIterationParams(Vector3 curr_key, bool cell_intersects_search_area) : this()
        {
            this.cell_key = curr_key;
            this.cell_intersects_search_area = cell_intersects_search_area;
        }
    }

    public IEnumerable<SparseGridIterationParams> Get_Cells_In_Area(Bounds search_area){
        Vector3 min_cell = GetCellKey_ForPosition(search_area.min);
        Vector3 max_cell = GetCellKey_ForPosition(search_area.max);
        for (int x = (int) min_cell.x; x <= (int) max_cell.x; x++){
            bool x_edge = x == min_cell.x || x == max_cell.x;

            for (int y = (int) min_cell.y; y <= (int) max_cell.y; y++){
                bool y_edge = y == min_cell.y || y == max_cell.y;

                for (int z = (int) min_cell.z; z <= (int) max_cell.z; z++){

                    Vector3 curr_key = new Vector3(x, y, z); 
                    bool z_edge = z == min_cell.z || z == max_cell.z;
                    bool cell_intersects_search_area = x_edge || y_edge || z_edge;

                    yield return new SparseGridIterationParams(curr_key, cell_intersects_search_area);

                }
            }
        }
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


        foreach(var c_cell_params in Get_Cells_In_Area(search_area)){
            cells_checked_iterations++;
            if(grid.ContainsKey(c_cell_params.cell_key) == false){
                continue;
            }

            
            if(c_cell_params.cell_intersects_search_area)
            {
                intersecting_cells_iterations++;
                // We're intersecting, so we need to check which elements from this grid cell are actually inside the serach area
                foreach (var c_elem in grid[c_cell_params.cell_key].elements)
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
                result.AddRange(grid[c_cell_params.cell_key].elements);
            }
        }
        return result;
    }
        
    public void Search_FullyContained_Cells(Bounds search_area){

    }

    
}
