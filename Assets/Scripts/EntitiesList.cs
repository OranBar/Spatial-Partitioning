using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using OBLib.QuadTree;
using UnityEngine;

using Square = OBLib.QuadTree.Square;

public class EntitiesList : MonoBehaviour
{
	private List<GameObject> objects;
    [Auto]
    private BoxCollider2D spawnArea;
    [Auto]
    private ObjectsSpawner spawner;

    public bool activate_search;
    public Collider2D search_collider;

    [ShowNativeProperty]
    public int Objects_count => objects?.Count ?? 0;

    // Start is called before the first frame update
    void Awake()
    {
		objects = new List<GameObject>(); 
        spawner.OnObjectSpawned += AddSpawnedObj_ToQuadTree;
    }

    void AddSpawnedObj_ToQuadTree(GameObject obj){
        objects.Add(obj);
    }

    private List<GameObject> prev_search_results = new List<GameObject>();

    public void Update(){
        if(activate_search)
        {
            LinearSearch();
        }
    }

    public int search_iterations = 0;

    List<long> search_time_measurements = new List<long>();

    [ShowNativeProperty]
    public double Search_Time_Average => search_time_measurements.Average();

	private List<GameObject> Search(Square search_area){
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
		List<GameObject> result = new List<GameObject>();
        search_iterations = 0;
		foreach(var c_obj in objects){
			if(search_area.Intersects((Vector2)c_obj.transform.position, c_obj.transform.lossyScale.x/2f )){
				result.Add(c_obj);
			}
            search_iterations++;
		}

        sw.Stop();
        if(search_time_measurements.Count >= 60){
            search_time_measurements.RemoveAt(0);
        }
        search_time_measurements.Add(sw.ElapsedMilliseconds);
        UnityEngine.Debug.Log("QuadTree Search: " + sw.Elapsed);
        
		return result;
	}
    
    private void LinearSearch()
    {
        Vector2 center = new Vector2(search_collider.bounds.center.x, search_collider.bounds.center.y);
        Square search_area = new Square(center, search_collider.bounds.extents.x);

		List<GameObject> results = Search(search_area);

        if (prev_search_results != null)
        {
            foreach (var c_result in prev_search_results)
            {
                c_result.GetComponent<SpriteRenderer>().color = Color.cyan;
            }
        }

        if (results != null)
        {
            foreach (var c_result in results)
            {
                c_result.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        prev_search_results = results;
    }

    void OnDrawGizmos(){
        if(Application.isPlaying == false){ return; }

    }

}
