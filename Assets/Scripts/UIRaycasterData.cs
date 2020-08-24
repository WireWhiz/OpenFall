using System;
using System.Net;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct UIRaycasterData : IComponentData
{
    public Hand hand;
    public UnityEngine.LayerMask layersToHit;
    public enum Hand
    {
        left,
        right
    }
    public CollisionFilter filter
    {
        get
        {
            return new CollisionFilter()
            {
                BelongsTo = (uint)(layersToHit.value),
                CollidesWith = (uint)(layersToHit.value)
            };
        }
    }
    public bool pressed;
}
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class UIRaycastSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

        Entities.WithStructuralChanges().ForEach((ref UIRaycasterData raycasterData, in LocalToWorld transform)=>
        {
            float triggerWeight = 0;
            switch (raycasterData.hand)
            {
                case UIRaycasterData.Hand.left:
                    triggerWeight = InputManager.pilotInput.lTrigger;
                    break;
                case UIRaycasterData.Hand.right:
                    triggerWeight = InputManager.pilotInput.rTrigger;
                    break;
            }
            if(triggerWeight > .25f)
            {
                if (!raycasterData.pressed)
                {
                    raycasterData.pressed = true;
                    RaycastInput input = new RaycastInput
                    {
                        Start = transform.Position,
                        End = transform.Position + transform.Forward * 3,
                        Filter = raycasterData.filter
                    };
                    if (collisionWorld.CastRay(input, out RaycastHit hit))
                    {
                        if (entityManager.HasComponent<UIButton>(hit.Entity))
                        {
                            entityManager.AddComponent<UIButtonPress>(hit.Entity);
                            entityManager.SetComponentData(hit.Entity, new UIButtonPress { hit = hit });

                        }
                    }
                }
            }
            else
            {
                raycasterData.pressed = false;
            }
        }).Run();
    }

}