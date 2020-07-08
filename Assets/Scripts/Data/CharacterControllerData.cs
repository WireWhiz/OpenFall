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
    public float maxAngle;
    public bool onGround;
    public float3 groundNormal;
    public float3 footOffset;
    public float3 moveDelta;
    public LayerMask layersToIgnore;
    public float3 center => footOffset + new float3(0, height / 2, 0);
    public float3 vertexTop => footOffset + new float3(0, height - raduis, 0);
    public float3 vertexBottom => footOffset + new float3(0, raduis, 0);
    public float3 top => footOffset + new float3(0, height, 0);
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
