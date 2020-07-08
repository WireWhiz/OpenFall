using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(CharacterControllerSystem))]
public class PlayerMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        EntityManager entityManager = EntityManager;
        var input = GameController.pilotInput;
        JobHandle job = Entities.ForEach((DynamicBuffer<BounceNormals> bounceNormalsBuffer, ref PlayerMovementData playerMovement, ref CharacterControllerData characterController, ref Translation translation, in CameraRigData cameraRig) =>
        {
        //We need to get the rotation of our controller so that we can change our input by it
            quaternion controlRotation = quaternion.identity;
            switch (playerMovement.controlType)
            {
                case PlayerMovementData.ControlType.localToHead:
                    controlRotation = entityManager.GetComponentData<LocalToWorld>(cameraRig.head).Rotation.ProjectOnPlane(new float3(0, 1, 0));
                    break;
                case PlayerMovementData.ControlType.localToHand:
                    controlRotation = entityManager.GetComponentData<LocalToWorld>(cameraRig.leftHand).Rotation.ProjectOnPlane(new float3(0, 1, 0));
                    break;
            }
            for (int i = 0; i < bounceNormalsBuffer.Length; i++)
                playerMovement.velocity = playerMovement.velocity.ProjectOnPlane(bounceNormalsBuffer[i].Value);
            float3 movement;
            //UnityEngine.Debug.Log(characterController.groundNormal.ToString() + " " + characterController.groundNormal.AngleFrom(new float3(0, 1, 0)).ToString() + " " + math.cross(new float3(0, 1, 0), characterController.groundNormal).ToString());
            movement = math.mul(controlRotation, new float3(input.movement.x, 0, input.movement.y));

            playerMovement.velocity += (playerMovement.gravity) * deltaTime;
            if (characterController.onGround)
            {
                float speed = math.distance(playerMovement.velocity, float3.zero);
                if (speed != 0) // To avoid divide by zero errors
                {
                    float drop = speed * playerMovement.groundFriction * deltaTime;
                    playerMovement.velocity *= math.max(speed - drop, 0) / speed;
                }
                
                if (!movement.Equals(float3.zero))
                    playerMovement.velocity = ClampedAccelerate(math.normalize(movement), playerMovement.velocity, math.distance(movement, float3.zero) * playerMovement.acceletaion * deltaTime, playerMovement.maxSpeed);
                playerMovement.velocity = new float3(playerMovement.velocity.x, math.min(playerMovement.velocity.y, -playerMovement.groundingDistance), playerMovement.velocity.z);
                if(input.Jump)
                    playerMovement.velocity = new float3(playerMovement.velocity.x, math.max(playerMovement.velocity.y, math.sqrt(2 * playerMovement.jumpHeight * math.distance(float3.zero, playerMovement.gravity))), playerMovement.velocity.z);
            }
            else
            {
                if (!movement.Equals(float3.zero))
                {
                    playerMovement.velocity = ClampedAccelerate(math.normalize(movement), playerMovement.velocity, math.distance(movement, float3.zero) * playerMovement.airAcceleration * deltaTime, playerMovement.maxSpeed);
                }
            }
            
            characterController.moveDelta += playerMovement.velocity * deltaTime;
        }).Schedule(Dependency);
        job.Complete();
    }
    private static float3 ClampedAccelerate(float3 direction, float3 currentVelocity, float acceleration, float maxSpeed)
    {
        float projectedSpeed = math.dot(currentVelocity, direction);

        if (projectedSpeed + acceleration > maxSpeed)
            acceleration = maxSpeed - projectedSpeed;

        return currentVelocity + direction * acceleration;
    }
}
