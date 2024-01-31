using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class SparseGridTester : MonoBehaviour
{
    [Auto]
    private ObjectsSpawner spawner;
    private SparseGrid<GameObject> sparse_grid;

    void Awake()
    {
        sparse_grid = new SparseGrid<GameObject>();

        spawner.OnObjectSpawned += AddSpawnedObj_ToGrid;
    }

    void AddSpawnedObj_ToGrid(GameObject obj){
        Debug.Log("Add Spawned Obj");
        sparse_grid.Add(obj, obj.transform.position);
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
