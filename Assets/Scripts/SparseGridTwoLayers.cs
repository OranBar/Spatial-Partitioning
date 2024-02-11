using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowRes_SparseGridCell_ElementOperation<T> : ISparseGrid_ElementOperations<SparseGridCell<T>>
{
    public Bounds GetBoundingBox(SparseGridCell<T> obj)
    {
        return obj.bounds;
    }

    public Vector3 GetPosition(SparseGridCell<T> obj)
    {
        return obj.bounds.center;
    }
}

// public class LowRes_SparseGridCell<T> : SparseGridCell<T>
// {
//     public bool high_res_grid_contains_remaining_elements;

//     public LowRes_SparseGridCell(Vector3 center, int grid_cell_size) : base(center, grid_cell_size)
//     {
//     }
    
    
// }

public class SparseGridTwoLayers<T>
{

    private SparseGrid<SparseGridCell<T>> low_res_sparse_grid;
    private SparseGrid<T> high_res_sparse_grid;
    private ISparseGrid_ElementOperations<T> high_res_grid_operations;
    private LowRes_SparseGridCell_ElementOperation<T> low_res_grid_operations;
    // private int max_elements_low_res = 30;


    public SparseGridTwoLayers(int smallest_cell_size, ISparseGrid_ElementOperations<T> sparse_grid_operations)
    {
        this.high_res_grid_operations = sparse_grid_operations;
        this.low_res_grid_operations = new LowRes_SparseGridCell_ElementOperation<T>();
        
        low_res_sparse_grid = new SparseGrid<SparseGridCell<T>>(smallest_cell_size * 100, low_res_grid_operations);
        high_res_sparse_grid = new SparseGrid<T>(smallest_cell_size, sparse_grid_operations);
    }
    
    public void Add(T obj){

        List<SparseGridCell<T>> cells_where_obj_was_added = high_res_sparse_grid.Add(obj);
        foreach(var c_cell in cells_where_obj_was_added){
            low_res_sparse_grid.Add(c_cell); 
        }
        
        // SparseGridCell<T> grid_cell = low_res_sparse_grid.GetGridCell(obj);
        // if(grid_cell == null){
        //     low_res_sparse_grid.Add(obj);
        // }
        // else if(grid_cell.elements.Count < max_elements_low_res){
        //     low_res_sparse_grid.Add(obj);
        //     // Mark as full if we reached max_elements_low_rew, so we know we have to search the high_res
        // } else if(grid_cell.elements.Count >= max_elements_low_res){
        //     high_res_sparse_grid.Add(obj);
        // }
    }
    
    public void Remove(T obj){
        SparseGridCell<T> prev_obj_cell = high_res_sparse_grid.Remove(obj);
        if(prev_obj_cell.elements.Count == 0){
            low_res_sparse_grid.Remove(prev_obj_cell);
        }
    }

    private List<T> search_result = new List<T>();
    public List<T> Search(Bounds search_area){
        search_result.Clear();

        // TODO: Search by checking low_res grid, then follow up to the high_res from the references in the low_res. Do not access high_res directly
        List<SparseGridCell<T>> cells_to_check = low_res_sparse_grid.Search(search_area);

        foreach(var c_cell in cells_to_check){
            // If we're not intersecting, I'm assuming it means it's fully contained.
            // We wouldn't be iterating on it if it didn't contain at least 1 element, meaning they can't/shouldn't be disjointed
            bool is_cell_fully_contained_in_search_area = search_area.Intersects(c_cell.bounds) == false;

            is_cell_fully_contained_in_search_area = false;
            
            if(is_cell_fully_contained_in_search_area){
                
                search_result.AddRange(c_cell.elements);
                
            } else {

                foreach(var c_elem in c_cell.elements){
                    // Bounds c_elem_bounds = this.high_res_grid_operations.GetBoundingBox(c_elem);
                    Vector3 c_elem_pos = this.high_res_grid_operations.GetPosition(c_elem);
                    if(search_area.Contains(c_elem_pos)){
                        search_result.Add(c_elem); 
                    }
                }
            }
        }
        return search_result;
    }
}

// public class SparseGridTwoLayers<T>
// {

//     private SparseGrid<SparseGrid<T>> low_res_sparse_grid;
//     private ISparseGrid_ElementOperations<T> sparse_grid_operations;

//     // private SparseGrid<T> high_res_sparse_grid;

//     public SparseGridTwoLayers(int smallest_cell_size, ISparseGrid_ElementOperations<T> sparse_grid_operations)
//     {
//         // high_res_sparse_grid = new SparseGrid<T>(smallest_cell_size, sparse_grid_operations);
//         this.sparse_grid_operations = sparse_grid_operations;
//         low_res_sparse_grid = new SparseGrid<SparseGrid<T>>(smallest_cell_size * 100);
//     }
    
//     public void Add(T obj){
//         high_res_sparse_grid.Add(obj);
//     }
// }
