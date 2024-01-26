using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using BarbarO.ExtensionMethods;
using System;

public class ObjectsSpawner : MonoBehaviour
{
    public int objs_to_spawn;
    public GameObject obj_prefab;

    public event Action<GameObject> OnObjectSpawned = ( _ ) => { };

    [Auto]
    private BoxCollider2D spawnArea;

    [Button]
    void SpawnObjects()
    {
        for (int i = 0; i < objs_to_spawn; i++)
        {
            Vector3 new_point = spawnArea.bounds.RandomPointInBounds();
            GameObject new_go = Instantiate(obj_prefab, new_point, Quaternion.identity, this.transform);
            OnObjectSpawned.Invoke(new_go);
        }
    }
    
    [Button]
    void SpawnSingleObject(Vector3 targetPos)
    {
        GameObject new_go = Instantiate(obj_prefab, targetPos, Quaternion.identity, this.transform);
        OnObjectSpawned.Invoke(new_go);
    }


    public int spawn_rate_frames = 40;
    
    void Update(){
        if(Input.GetMouseButton(0)){
            if(Time.frameCount % spawn_rate_frames == 0){
                var world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                world_pos.z = 0f;
                SpawnSingleObject(world_pos);
            }
        }
    }

}
