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
        input.controller = GetComponent<PlayerInput>();
        input.controller.SwitchCurrentControlScheme("XR");
    }

    void Update()
    {
        SetCapsuleCenter(footPose - transform.position - movementSettings.footPose );
        Move(Quaternion.AngleAxis(rig.head.rotation.eulerAngles.y, Vector3.up) * new Vector3(input.movement.x, 0, input.movement.y) * Time.deltaTime + Vector3.down * 1.5f * Time.deltaTime);
    }

    public void Move(Vector3 delta)
    {
        Vector3 remander = delta;
        
        for(int i = 0; i < movementSettings.slideItterations; i++)
        {
            UpdateClambering(delta);
            Vector3 newRemander = CastXY(Vector3.ProjectOnPlane(remander, Vector3.up));
            newRemander += CastZ(Vector3.Project(remander + newRemander, Vector3.up));
            remander = newRemander;
            if(remander == new Vector3())
                break;
        }
        if(clambering && sliding)
        {
            sliding = false;
            CastZ(Vector3.up * (movementSettings.stepUpSpeed * Time.deltaTime));
            sliding = true;
        }

    }

    private void UpdateClambering(Vector3 delta)
    {
        sliding = false;
        clambering = false;
        if(Physics.SphereCast(movementSettings.colliderTop + delta.normalized * movementSettings.raduis - Vector3.up * movementSettings.raduis + transform.position, movementSettings.raduis, Vector3.down, out RaycastHit hit, movementSettings.height - movementSettings.raduis * 2))
        {
            Debug.Log(hit.point.y - transform.position.y - movementSettings.raduis);
            if (Vector3.Angle(Vector3.up, hit.normal) < movementSettings.maxAngle && hit.point.y - transform.position.y < movementSettings.maxStepHeight)
                clambering = true;
        }
    }

    private Vector3 CastXY(Vector3 delta)
    {
        if (Physics.CapsuleCast(movementSettings.colliderBotom + Vector3.up * (movementSettings.raduis - movementSettings.skin) + transform.position, movementSettings.colliderTop + transform.position - Vector3.up * (movementSettings.raduis - movementSettings.skin), movementSettings.raduis - movementSettings.skin, delta.normalized, out RaycastHit hit, delta.magnitude + movementSettings.skin))
        {
            sliding = true;
            transform.position += delta.normalized * (hit.distance - movementSettings.skin);
            return Vector3.ProjectOnPlane( Vector3.ProjectOnPlane(delta.normalized * (delta.magnitude - (hit.distance - movementSettings.skin)), GetColliderNormal(hit.point)), Vector3.up);  
        }
        else
        {
            transform.position += delta;
        }

        return new Vector3();
    }
    private Vector3 CastZ(Vector3 delta)
    {
        onGround = false;
        if (sliding && clambering)
            return new Vector3();
        if (Physics.CapsuleCast(movementSettings.colliderBotom + Vector3.up * (movementSettings.raduis - movementSettings.skin) + transform.position, movementSettings.colliderTop + transform.position - Vector3.up * (movementSettings.raduis - movementSettings.skin), movementSettings.raduis - movementSettings.skin, delta.normalized, out RaycastHit hit, delta.magnitude + movementSettings.skin))
        {
            transform.position += delta.normalized * (hit.distance - movementSettings.skin);
            //Debug.Log(Vector3.Angle(Vector3.down, GetColliderNormal(hit.point)));
            if(Vector3.Angle(Vector3.down, GetColliderNormal(hit.point)) > movementSettings.maxAngle)
            {
                onGround = true;
                return Vector3.Project(-Vector3.ProjectOnPlane(delta.normalized * (delta.magnitude - (hit.distance - movementSettings.skin)), GetColliderNormal(hit.point)), Vector3.up);
            } 
        }
        else
        {
            transform.position += delta;
        }

        return new Vector3();
    }
    public void SetCapsuleCenter(Vector3 delta)
    {
        if (Physics.CapsuleCast(movementSettings.colliderBotom + Vector3.up * (movementSettings.raduis - movementSettings.skin) + transform.position, movementSettings.colliderTop + transform.position - Vector3.up * (movementSettings.raduis - movementSettings.skin), movementSettings.raduis - movementSettings.skin, delta, out RaycastHit hit, delta.magnitude + movementSettings.skin))
        {
            
            Move(-delta);
            movementSettings.footPose += delta;
            Move(delta);
            Debug.DrawRay(movementSettings.footPose + transform.position + Vector3.up * movementSettings.height / 2, -delta.normalized * delta.magnitude, Color.blue, 3f);
            Debug.DrawRay(movementSettings.footPose + transform.position + Vector3.up * movementSettings.height / 2, Vector3.up, Color.red, 3f);
        }
        else
        {
            //Debug.Log(movementSettings.footPose);
             movementSettings.footPose += delta;

        }
    }
    public Vector3 GetColliderNormal(Vector3 hit)
    {
        Vector3 normal = (Vector3.ClampMagnitude(Vector3.Project(hit - (movementSettings.footPose + transform.position + Vector3.up * movementSettings.height / 2), Vector3.up), movementSettings.height / 2) + transform.position + movementSettings.footPose);
        Debug.DrawLine(hit , normal + transform.position + movementSettings.footPose, Color.green);
        return new Vector3(normal.x, normal.y, normal.z) - hit;
    }
    [System.Serializable]
    public struct MovementSettings
    {
        public float speed;
        public float height;
        public float raduis;
        public float skin;
        public float maxAngle;
        public float maxStepHeight;
        public float stepUpSpeed;
        public int slideItterations;
        [HideInInspector]
        public Vector3 footPose;
        public Vector3 colliderTop
        {
            get
            {
                return footPose + Vector3.up * (height - skin);
            }
        }
        public Vector3 colliderBotom
        {
            get
            {
                return footPose + Vector3.up * skin;
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        DebugExtension.DrawCapsule(movementSettings.colliderBotom + transform.position, movementSettings.colliderTop + transform.position, Color.blue, movementSettings.raduis - movementSettings.skin);
        DebugExtension.DrawCapsule(movementSettings.footPose + transform.position, movementSettings.footPose + Vector3.up * movementSettings.height + transform.position, Color.green, movementSettings.raduis);
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
    }
}
