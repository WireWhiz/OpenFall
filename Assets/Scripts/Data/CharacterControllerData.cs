using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct CharacterControllerData : IComponentData
{
    public float raduis;
    public float height;
    public float skin;
    public int lod;
    public float maxAngle;
    public bool onGround;
    public float3 footOffset;
    public float3 moveDelta;
    public float3 lastMoveDelta;
    public LayerMask layersToIgnore;
    public float3 vertexTop
    {
        get
        {
            return footOffset + new float3(0, height - raduis, 0);
        }
    }
    public float3 vertexBottom
    {
        get
        {
            return footOffset + new float3(0, raduis, 0);
        }
    }
    public float3 top
    {
        get
        {
            return footOffset + new float3(0, height, 0);
        }
    }
    public CollisionFilter Filter
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
