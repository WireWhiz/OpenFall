using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class HeadTracking : MonoBehaviour
{
    public static HeadTracking instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        instance = this;
        
    }
}
