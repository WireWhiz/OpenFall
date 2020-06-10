using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(TransformInputSystem))]
public class HeadSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Transform head = HeadTracking.instance.transform;
        float3 pos = new float3();
        quaternion rot = new quaternion();
        EntityManager manager = EntityManager;
        Entities.WithAll<HeadData>().ForEach((Parent parent, in Translation translation, in Rotation rotation) => {
            float4x4 transformMatrix = manager.GetComponentData<LocalToWorld>(parent.Value).Value;
            pos = math.transform(transformMatrix, translation.Value);
            rot = math.mul(rotation.Value, new quaternion(transformMatrix));
        }).Run();
        head.position = pos;
        head.rotation = rot;
        return inputDeps;
    }
}
