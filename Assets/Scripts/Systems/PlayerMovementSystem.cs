using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(CharacterControllerSystem))]
public class PlayerMovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        EntityManager entityManager = EntityManager;
        Entities.ForEach((DynamicBuffer<BounceNormals> bounceNormalsBuffer, ref PlayerMovementData playerMovement, ref CharacterControllerData characterController, ref Translation translation,  in PlayerInputData input) =>
        {
            //playerMovement.velocity = characterController.lastMoveDelta / deltaTime;
            for (int i = 0; i < bounceNormalsBuffer.Length; i++)
                playerMovement.velocity = playerMovement.velocity.ProjectOnPlane(bounceNormalsBuffer[i].Value);
            playerMovement.velocity += (playerMovement.gravity + new float3(input.movement.x, 0, input.movement.y) * playerMovement.movementSpeed) * deltaTime;
            characterController.moveDelta += playerMovement.velocity * deltaTime;
        }).Run();
        return inputDeps;
    }
}
