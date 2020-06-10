using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PilotController : MonoBehaviour
{
    public Rig rig;
    public MovementSettings movementSettings;
    private Input input;
    
    [HideInInspector]
    public Vector3 velocity;
    public bool onGround;
    public bool clambering;

    private CharacterController characterController;
    private bool sliding;
    public Vector3 footPose
    {
        get
        {
            return Vector3.ProjectOnPlane(rig.head.position - transform.position, Vector3.up) + transform.position;
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        input.controller = GetComponent<PlayerInput>();
        input.controller.SwitchCurrentControlScheme("XR");
    }

    void Update()
    {
        UpdateCapsuleCenter();
        Move();
    }

    private void UpdateCapsuleCenter()
    {
        float height = rig.head.position.y - transform.position.y;
        characterController.height = height;
        characterController.center = transform.InverseTransformPoint(footPose) + Vector3.up * (height / 2);
    }

    public void Move()
    {
        velocity += Vector3.down * 9.8f * Time.deltaTime;
        velocity = Accelerate(Quaternion.AngleAxis(rig.head.rotation.eulerAngles.y, Vector3.up) * new Vector3(input.movement.x, 0, input.movement.y), velocity, movementSettings.airAcceleration, movementSettings.maxSpeed);
        characterController.Move(velocity * Time.deltaTime);
    }

    public Vector3 Accelerate(Vector3 direction, Vector3 oldVeloctiy, float accelerate, float maxVelocity)
    {
        float pVelocity = Vector3.Dot(oldVeloctiy, direction);
        float acceleration = accelerate * Time.deltaTime;

        if (pVelocity + acceleration > maxVelocity)
            acceleration = maxVelocity - pVelocity;

        return oldVeloctiy + direction.normalized * acceleration;
    }
    [Serializable]
    public struct MovementSettings
    {
        public float maxSpeed;
        public float groundAccleration;
        public float airAcceleration;
    }
    private struct Input
    {
        public  PlayerInput controller;
        public Vector2 movement;
        public bool jump;
    }

    [Serializable]
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
    }
}
