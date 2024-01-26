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
    
    void Start(){
        //SpawnObjects();
    }

}
