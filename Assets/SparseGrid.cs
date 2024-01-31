using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;

public class SparseGrid
{
    public class SparseGridCell {
        public List<GameObject> elements;
        public Bounds bounds;

        public SparseGridCell(Vector3 center)
        {
            elements = new List<GameObject>();
            bounds = new Bounds(
                center, 
                Vector3.one * grid_cell_size
            );
        }
        
    }


    public Dictionary<Vector3, SparseGridCell> grid;
    private const int grid_cell_size = 10; 
    // private const int grid_cell_size = 1; 
    // private const int grid_cell_size = 10000; 

    public SparseGrid()
    {
        this.grid = new Dictionary<Vector3, SparseGridCell>();
    }

    public void Add(GameObject obj_to_add, Vector3 pos){
        Vector3 obj_grid_cell = GetCellKey_ForPosition(pos);

        if(grid.ContainsKey(obj_grid_cell) == false){
            grid[obj_grid_cell] = new SparseGridCell(pos);
        }

        grid[obj_grid_cell].elements.Add(obj_to_add);
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
    
    public void Remove(GameObject obj_to_remove, Vector3 pos){
        Vector3 obj_grid_cell = GetCellKey_ForPosition(pos);

        if(grid[obj_grid_cell] == null){
            throw new System.Exception("Object not in grid");
        }

        grid[obj_grid_cell].elements.Remove(obj_to_remove);
    }
    
    public List<GameObject> Search(Bounds search_area){
        List<GameObject> result = new List<GameObject>();

        // List<SparseGridCell> cells_to_check = new List<SparseGridCell>();
        Vector3 min_cell = GetCellKey_ForPosition(search_area.min);
        Vector3 max_cell = GetCellKey_ForPosition(search_area.max);
        for (int x = (int) min_cell.x; x <= (int) max_cell.x; x++){
            for (int y = (int) min_cell.y; y <= (int) max_cell.y; y++){
                for (int z = (int) min_cell.z; z <= (int) max_cell.z; z++){

                    Vector3 curr_key = new Vector3(x, y, z); 
                    if(grid.ContainsKey(curr_key)){
                        foreach(var c_elem in grid[curr_key].elements){
                            if(search_area.Contains( c_elem.transform.position )){
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
