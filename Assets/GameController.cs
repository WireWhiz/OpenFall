using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public InputActionAsset input;
    void Start()
    {
        if(instance && instance != this)
        {
            Destroy(this);
        }
        instance = this;
    }
}
