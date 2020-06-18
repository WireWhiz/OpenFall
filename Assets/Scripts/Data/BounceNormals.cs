using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


[InternalBufferCapacity(7)]
public struct BounceNormals : IBufferElementData
{
    public float3 Value;
}
