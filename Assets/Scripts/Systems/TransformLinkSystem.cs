using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class TransformLinkSystem : SystemBase
{
    protected override void OnUpdate()
    {
        /*
        Transform target = TransformLink.instance.transform;
        float3 pos = new float3();
        quaternion rot = new quaternion();
        EntityManager manager = EntityManager;
        Entities.WithAll<TransformLinkData>().ForEach((in Translation translation, in Rotation rotation) => {
            pos = translation.Value;
            rot = rotation.Value;
        }).Run();
        Entities.WithAll<TransformLinkData>().ForEach((Parent parent, in Translation translation, in Rotation rotation) => {
            float4x4 transformMatrix = manager.GetComponentData<LocalToWorld>(parent.Value).Value;
            pos = math.transform(transformMatrix, translation.Value);
            rot = math.mul(rotation.Value, new quaternion(transformMatrix));
        }).Run();
        target.position = pos;
        target.rotation = rot;
        
        */
    }
}
