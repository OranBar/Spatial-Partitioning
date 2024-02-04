using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NaughtyAttributes;
using UnityEngine;

public class GameObjectPositionGetter : ISparseGrid_ElementOperations<GameObject>
{
    public Vector3 GetPosition(GameObject obj)
    {
        return obj.transform.position;
    }
    
    public Bounds GetBoundingBox(GameObject obj){
        return default;
    }
}

public class SparseGridTester : MonoBehaviour
{
    [Auto]
    private ObjectsSpawner spawner;
    private SparseGridTwoLayers<GameObject> sparse_grid;
    [Auto]
    private BoxCollider spawnArea;
    [Auto]

    public int grid_cell_size;
    public bool activate_search;
    public Collider search_collider;

    void Awake()
    {
        sparse_grid = new SparseGridTwoLayers<GameObject>(grid_cell_size, new GameObjectPositionGetter());

        spawner.OnObjectSpawned += AddSpawnedObj_ToGrid;
    }

    void AddSpawnedObj_ToGrid(GameObject obj){
        UnityEngine.Debug.Log("Add Spawned Obj");
        sparse_grid.Add(obj, obj.transform.position);
    }
    
        public void Update(){
        if(activate_search)
        {
            ClearPrevSearchColors(); 
            SparseGridSearch();
        }
    }
    
    List<long> search_time_measurements = new List<long>();
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
        if(search_time_measurements.Count >= 60){
            search_time_measurements.RemoveAt(0);
        }
        search_time_measurements.Add(sw.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Linear Search: " + sw.Elapsed);
        UnityEngine.Debug.Log($"Cell iters = {sparse_grid.cell_iterations} | Element iters = {sparse_grid.element_iterations}");
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
}
