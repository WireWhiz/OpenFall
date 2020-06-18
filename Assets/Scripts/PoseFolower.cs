using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PoseFolower : MonoBehaviour
{
    public void UpdatePosition(InputAction.CallbackContext callback)
    {
        transform.localPosition = callback.ReadValue<Vector3>();
    }
    public void UpdateRotation(InputAction.CallbackContext callback)
    {
        transform.localRotation = callback.ReadValue<Quaternion>();
    }
}
