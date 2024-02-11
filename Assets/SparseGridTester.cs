using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class BoxCollidersCache{
    public static Dictionary<GameObject, BoxCollider> obj_to_collider_map = new Dictionary<GameObject, BoxCollider>();
}

public class GameObjectPositionGetter : ISparseGrid_ElementOperations<GameObject>
{
    public Vector3 GetPosition(GameObject obj)
    {
        return obj.transform.position;
    }
    
    public Bounds GetBoundingBox(GameObject obj){
        Bounds value;
        if(BoxCollidersCache.obj_to_collider_map.ContainsKey(obj) == false){
            BoxCollidersCache.obj_to_collider_map[obj] = obj.GetComponent<BoxCollider>();
        }
        return BoxCollidersCache.obj_to_collider_map[obj].bounds;
    }
}

public class SparseGridTester : MonoBehaviour
{
    [Auto]
    private ObjectsSpawner spawner;
    private SparseGrid<GameObject> sparse_grid;
    [Auto]
    private BoxCollider spawnArea;
    [Auto]

    public int grid_cell_size;
    public bool activate_search;
    public Collider search_collider;

    void Awake()
    {
        sparse_grid = new SparseGrid<GameObject>(grid_cell_size, new GameObjectPositionGetter());

        spawner.OnObjectSpawned += AddSpawnedObj_ToGrid;
        foreach(Transform child in this.transform){
            AddSpawnedObj_ToGrid(child.gameObject);
        }
    }

    void AddSpawnedObj_ToGrid(GameObject obj){
        UnityEngine.Debug.Log("Add Spawned Obj");
        sparse_grid.Add(obj);
    }
    
        public void Update(){
        if(activate_search)
        {
            ClearPrevSearchColors(); 
            SparseGridSearch();
        }
    }
    
    List<long> search_time_measurements = new List<long>();
    public double search_time_average = 0;
    // [ShowNativeProperty]
    // public double Search_Time_Average => search_time_measurements.IsNullOrEmpty() ? 0 : search_time_measurements.Average();
    private List<GameObject> prev_search_results = new List<GameObject>();
    
    private List<GameObject> SparseGridSearch()
    {
        var sw = new Stopwatch();
        sw.Start();

        IEnumerable<GameObject> results = sparse_grid.Search(search_collider.bounds);

        prev_search_results.Clear();
        if (results != null)
        {
            foreach (var c_result in results)
            {
                c_result.GetComponent<MeshRenderer>().material.color = Color.red;
                prev_search_results.Add(c_result);
            }
        }

        sw.Stop();
        while(search_time_measurements.Count >= 60){
            search_time_measurements.RemoveAt(0);
        }
        search_time_measurements.Add(sw.ElapsedMilliseconds);
        search_time_average = search_time_measurements.IsNullOrEmpty() ? 0 : search_time_measurements.Average();
        UnityEngine.Debug.Log("Linear Search: " + sw.Elapsed);
        UnityEngine.Debug.Log($"Cell iters = {sparse_grid.cells_checked_iterations} | Element iters = {sparse_grid.element_iterations} | Elements Found {prev_search_results.Count} | Intersecting Cells {sparse_grid.intersecting_cells_iterations} | Contained Cells {sparse_grid.contained_cells_iterations}");
        return prev_search_results;
    }

    private void ClearPrevSearchColors(){
        if (prev_search_results != null)
        {
            foreach (var c_result in prev_search_results)
            {
                c_result.GetComponent<MeshRenderer>().material.color = Color.cyan;
            }
        }

    }



    
    private Transform random_obj;
    [Button]
    public void Select(){
        if(random_obj != null){
            random_obj.GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
        random_obj = this.transform.GetChildren().GetRandomElement();
        random_obj.GetComponent<MeshRenderer>().material.color = Color.yellow;
    }

    [Button]
    public void TestRemove(){
        sparse_grid.Remove(random_obj.gameObject, random_obj.position);
        random_obj.GetComponent<MeshRenderer>().material.color = Color.gray;
        Destroy(random_obj);
        random_obj = null;
    }
    
    [Button]
    public void TestSearch(){
        // var results = QuadTreeSearch();
    }

    public bool draw_cubes = false;
    void OnDrawGizmos(){
        if(draw_cubes == false){
            return;
        }
        if(sparse_grid?.grid == null){
            return;
        }
        Gizmos.color = Color.yellow;
        foreach(var c_cell in sparse_grid.grid.Values){
            Gizmos.DrawWireCube(c_cell.bounds.center, c_cell.bounds.size);
        }
    }
}
