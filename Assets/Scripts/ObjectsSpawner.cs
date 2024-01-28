using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using BarbarO.ExtensionMethods;
using System;

public class ObjectsSpawner : MonoBehaviour
{
    public bool spawn_on_start;
    public int objs_to_spawn;
    public GameObject obj_prefab;

    public event Action<GameObject> OnObjectSpawned = ( _ ) => { };

    [Auto]
    private BoxCollider2D spawnArea;

    void Start(){
        if(spawn_on_start){
            SpawnObjects();
        }
    }

    [Button]
    void SpawnObjects()
    {
        for (int i = 0; i < objs_to_spawn; i++)
        {
            Vector3 new_point = spawnArea.bounds.RandomPointInBounds();
            GameObject new_go = SpawnSingleObject(new_point);
        }
    }
    
    GameObject SpawnSingleObject(Vector3 targetPos)
    {
        GameObject new_go = Instantiate(obj_prefab, targetPos, Quaternion.identity, this.transform);
        OnObjectSpawned.Invoke(new_go);
        last_spawn_time = Time.time;
        return new_go;
    }

    public void Awake(){
        Application.targetFrameRate = -1;
    }

    public float last_spawn_time = 0;
    public float spawn_cooldown = 0.10f;
    
    void Update(){
        if(last_spawn_time + spawn_cooldown >= Time.time ){
            return;
        }
        if(Input.GetMouseButton(0)){
            var world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world_pos.z = 0f;
            SpawnSingleObject(world_pos);
        }
    }

}
