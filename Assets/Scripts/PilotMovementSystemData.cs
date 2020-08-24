using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Unity.Physics;

[GenerateAuthoringComponent]
public struct PilotMovementSystemData : IComponentData
{
    [GhostDefaultField(1000)]
    public float3 velocity;
    [GhostDefaultField]
    public bool onGround;
    [GhostDefaultField]
    public bool jumpCooldown;
    public LayerMask layersToIgnore;
    [GhostDefaultField]
    public MovementMode movementMode;
    public enum MovementMode
    {
        walking,
        jumping,
        sliding,
        secondJumping,
        wallRunning
    };
    public CollisionFilter filter
    {
        get
        {
            return new CollisionFilter()
            {
                BelongsTo = (uint)(~layersToIgnore.value),
                CollidesWith = (uint)(~layersToIgnore.value)
            };
        }
    }
}
