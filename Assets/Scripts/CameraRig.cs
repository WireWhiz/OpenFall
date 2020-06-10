using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public static CameraRig instance;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    private void Awake()
    {
        if(instance == null && instance != this)
        {
            Destroy(this);
        }
        instance = this;
    }
}
