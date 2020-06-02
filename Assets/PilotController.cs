using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PilotController : MonoBehaviour
{
    public Rig rig;
    private Input input;

    void Start()
    {
        input.controller = GetComponent<PlayerInput>();
        input.controller.SwitchCurrentControlScheme("XR");
    }

    void Update()
    {
        transform.position += Quaternion.AngleAxis(rig.head.rotation.eulerAngles.y, Vector3.up) * new Vector3(input.movement.x * Time.deltaTime, 0, input.movement.y*Time.deltaTime);
    }

    private struct Input
    {
        public  PlayerInput controller;
        public Vector2 movement;
        public bool jump;
    }

    [System.Serializable]
    public struct Rig
    {
        public Transform root;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;
    }

    public void UpdateMovementInput(InputAction.CallbackContext callback)
    {
        input.movement = callback.ReadValue<Vector2>();
        Debug.Log(callback.ReadValue<Vector2>());
        Debug.Log(callback.valueType);
    }
}
