using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
/*
[UpdateBefore(typeof(CharacterControllerSystem))]
public class BodyColliderSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        Entities.ForEach((Entity entity, ref CharacterControllerData characterController,ref Translation translation, in LocalToWorld transform, in CameraRigData cameraRig) => {

            characterController.footOffset = (entityManager.GetComponentData<LocalToWorld>(cameraRig.head).Position - transform.Position).ProjectOnPlane(new float3(0,1,0));
            //UnityEngine.Debug.Log(characterController.footOffset.y);
            
        }).Run();
    }
}
*/