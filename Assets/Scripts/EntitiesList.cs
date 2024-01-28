using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Awake()
    {
		objects = new List<GameObject>(); 
        spawner.OnObjectSpawned += AddSpawnedObj_ToQuadTree;
    }

    void AddSpawnedObj_ToQuadTree(GameObject obj){
        Debug.Log("Add Spawned Obj");
		objects.Add(obj);
    }

    private List<GameObject> prev_search_results = new List<GameObject>();

    public void Update(){
        if(activate_search)
        {
            LinearSearch();
        }
    }
    
	private List<GameObject> Search(Square search_area){
		List<GameObject> result = new List<GameObject>();
		foreach(var c_obj in objects){
			if(search_area.Intersects((Vector2)c_obj.transform.position, c_obj.transform.lossyScale.x/2f )){
				result.Add(c_obj);
			}
		}

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
