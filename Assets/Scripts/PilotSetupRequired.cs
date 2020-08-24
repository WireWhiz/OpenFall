using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct PilotSetupRequired : IComponentData
{
    public Entity prefab;
}

[UpdateInGroup(typeof(ClientAndServerSimulationSystemGroup))]
public class PilotSetup : SystemBase 
{ 
    protected override void OnUpdate()
    {
        var entityManger = EntityManager;
        Entities.WithStructuralChanges().ForEach((Entity entity, in PilotSetupRequired setup) =>
        {
            Entity cameraRig = entityManger.Instantiate(setup.prefab);
            entityManger.AddComponent<Parent>(cameraRig);
            entityManger.AddComponent<LocalToParent>(cameraRig);
            entityManger.SetComponentData(cameraRig, new Parent { Value = entity});

            entityManger.SetComponentData(entity, new CameraRigChild { Value = cameraRig });

            UnityEngine.Debug.Log("set up pilot on world: " + entityManger.World.Name);
            entityManger.RemoveComponent<PilotSetupRequired>(entity);

        }).Run();
    }
}
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class CameraRigLinker : SystemBase
{
    protected override void OnUpdate()
    {
        var entityManger = EntityManager;
        if (CameraRigEntity.Value == Entity.Null)
            Entities.WithoutBurst().ForEach((Entity entity, in PilotData pilot, in CameraRigChild cameraRig) =>
            {
                if (pilot.clientHasAuthority)
                {
                    CameraRigEntity.Value = entity;
                    CameraRigEntity.World = entityManger.World;
                }
            }).Run();
    }
}