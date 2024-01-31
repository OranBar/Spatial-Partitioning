using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTesta : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GetComponent<BoxCollider>().bounds.extents);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
