using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/*[UpdateBefore(typeof(CharacterControllerSystem))]
public class PlayerMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        return;
        float deltaTime = Time.DeltaTime;
        EntityManager entityManager = EntityManager;
        var input = InputManager.pilotInput;
        var collisionWorld = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
        Entities.WithReadOnly(collisionWorld).ForEach((DynamicBuffer<BounceNormals> bounceNormalsBuffer, ref PlayerMovementData playerMovement, ref CharacterControllerData characterController, ref Translation translation, in CameraRigData cameraRig) =>
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

            float3 wallNormal = GetWallNormal(bounceNormalsBuffer.Reinterpret<float3>(), playerMovement.velocity, playerMovement.settings.maxWallAngle);
            if (wallNormal.Equals(float3.zero) && playerMovement.wallrunning)
            {
                float3 alternateWallNormal = SnapToWall(playerMovement.lastWallNormal * playerMovement.settings.wallSnappingDistance, ref translation, playerMovement, characterController, collisionWorld);
                if (!alternateWallNormal.Equals(float3.zero))
                    wallNormal = alternateWallNormal;
            }

            if (!input.Jump)
                playerMovement.jumpCooldown = false;

            if (!wallNormal.Equals(float3.zero))
            {
                //UnityEngine.Debug.Log(wallNormal.ToString());
                if (!playerMovement.wallrunning)
                {
                    float3 headUp = math.mul(entityManager.GetComponentData<LocalToWorld>(cameraRig.head).Rotation, new float3(0, 1, 0));
                    float headAngle = headUp.ProjectOnPlane(math.cross(new float3(0, 1, 0), wallNormal)).AngleFrom(new float3(0, 1, 0));

                    if (!characterController.onGround && headAngle > math.radians(playerMovement.settings.runActivationHeadAngle) && math.dot(headUp, wallNormal) < 0 && math.dot(wallNormal, playerMovement.lastWallNormal) < .9f)
                    {
                        playerMovement.wallrunning = true;
                        UnityEngine.Debug.Log("Started wallruning");
                    }
                }
                if (playerMovement.wallrunning)
                {
                    playerMovement.secondJumping = false;
                    playerMovement.velocity = playerMovement.velocity.ProjectOnPlane(new float3(0, 1, 0));
                    //UnityEngine.Debug.Log("wallruning");
                    float speed = math.distance(playerMovement.velocity, float3.zero);
                    if (speed != 0) // To avoid divide by zero errors
                    {
                        float drop = speed * playerMovement.settings.wallRunningFriction * deltaTime;
                        playerMovement.velocity *= math.max(speed - drop, 0) / speed;
                    }
                    
                    if (input.Jump && !playerMovement.jumpCooldown)
                    {
                        playerMovement.wallrunning = false;
                        playerMovement.jumpCooldown = true;
                        Jump(ref playerMovement, movement, playerMovement.settings.jumpHeight, playerMovement.settings.jumpAcceleration, playerMovement.settings.maxWallSpeed);
                    }
                    else
                    {
                        float3 moveDir = movement.Project(math.cross(wallNormal, new float3(0, 1, 0)));
                        if (!movement.Equals(float3.zero))
                            playerMovement.velocity = ClampedAccelerate(math.normalize(moveDir), playerMovement.velocity, math.distance(moveDir, float3.zero) * playerMovement.settings.wallAcceleration, playerMovement.settings.maxWallSpeed).ProjectOnPlane(wallNormal);
                        //UnityEngine.Debug.Log(playerMovement.velocity.ToString());
                        //characterController.moveDelta += wallNormal * playerMovement.settings.wallSnappingDistance;
                        playerMovement.lastWallNormal = wallNormal;
                    }
                }


            }
            else
            {
                playerMovement.wallrunning = false;
            }

            if (!playerMovement.wallrunning)
            {
                
                playerMovement.velocity += (playerMovement.settings.gravity) * deltaTime;
                if (characterController.onGround)
                {
                    playerMovement.secondJumping = false;
                    playerMovement.lastWallNormal = float3.zero;
                    float speed = math.distance(playerMovement.velocity, float3.zero);
                    if (speed != 0) // To avoid divide by zero errors
                    {
                        float drop = speed * playerMovement.settings.groundFriction * deltaTime;
                        playerMovement.velocity *= math.max(speed - drop, 0) / speed;
                    }

                    if (!movement.Equals(float3.zero))
                        playerMovement.velocity = ClampedAccelerate(math.normalize(movement), playerMovement.velocity, math.distance(movement, float3.zero) * playerMovement.settings.acceleration * deltaTime, playerMovement.settings.maxSpeed);
                    playerMovement.velocity = new float3(playerMovement.velocity.x, math.min(playerMovement.velocity.y, -playerMovement.settings.groundingDistance), playerMovement.velocity.z);
                    
                    if (input.Jump && !playerMovement.jumpCooldown)
                    {
                        playerMovement.jumpCooldown = true;
                        Jump(ref playerMovement, float3.zero, playerMovement.settings.jumpHeight, playerMovement.settings.jumpAcceleration, playerMovement.settings.maxSpeed);
                    }
                }
                else
                {
                    if (!movement.Equals(float3.zero))
                    {
                        playerMovement.velocity = ClampedAccelerate(math.normalize(movement), playerMovement.velocity, math.distance(movement, float3.zero) * playerMovement.settings.airAcceleration * deltaTime, playerMovement.settings.maxSpeed);
                    }
                }
            }

            if (!playerMovement.wallrunning && !characterController.onGround && !playerMovement.jumpCooldown && !playerMovement.secondJumping && input.Jump)
            {
                playerMovement.secondJumping = true;
                playerMovement.jumpCooldown = true;
                Jump(ref playerMovement, movement, playerMovement.settings.secondJumpHeight, playerMovement.settings.jumpAcceleration, playerMovement.settings.maxSpeed);
            }

            characterController.moveDelta += playerMovement.velocity * deltaTime;

            //UnityEngine.Debug.Log(characterController.moveDelta.ToString());
        }).Run();//.Schedule(Dependency);
        //job.Complete();
    }
    private static float3 ClampedAccelerate(float3 direction, float3 currentVelocity, float acceleration, float maxSpeed)
    {
        float projectedSpeed = math.dot(currentVelocity, direction);

        if (projectedSpeed + acceleration > maxSpeed)
            acceleration = maxSpeed - projectedSpeed;

        return currentVelocity + direction * acceleration;
    }
    private unsafe static float3 SnapToWall(float3 delta, ref Translation translation, in PlayerMovementData playerMovement, in CharacterControllerInput characterController, in CollisionWorld collisionWorld)
    {
        RaycastInput collisionCheck = new RaycastInput()
        {
            Start = translation.Value + characterController.center + math.normalize(delta) * characterController.raduis * 0.9f,
            End = translation.Value + characterController.center + delta + math.normalize(delta) * characterController.raduis * 0.9f,
            Filter = characterController.filter
        };
        UnityEngine.Debug.DrawLine(collisionCheck.Start, collisionCheck.End, UnityEngine.Color.red, 4);
        if (collisionWorld.CastRay(collisionCheck, out RaycastHit hit))
        {
            if(playerMovement.velocity.AngleFrom(-hit.SurfaceNormal) < math.radians(playerMovement.settings.maxCornerAngle))
            translation.Value += (collisionCheck.End - collisionCheck.Start) * hit.Fraction;
            UnityEngine.Debug.Log("hit wall");
            return -hit.SurfaceNormal;
        }
        return float3.zero;
    }
    private static float3 GetWallNormal(DynamicBuffer<float3> bounces, float3 forward, float maxWallAngle)
    {
        forward = forward.ProjectOnPlane(new float3(0, 1, 0));
        float3 normal = new float3();
        float closest = -float.MaxValue;
        for (int i = 0; i < bounces.Length; i++)
        {
            if (bounces[i].AngleFrom(new float3(0, 1, 0)) - math.PI / 2 < math.radians(maxWallAngle))
            {
                if (math.dot(math.normalize(forward), math.normalize(bounces[i])) > closest)
                    normal = math.normalize(bounces[i]);
                else if (forward.Equals(float3.zero))
                    normal = math.normalize(bounces[i]);
            }
        }
        
        return normal;
    }
    private static void Jump(ref PlayerMovementData playerMovement, float3 movement, float height, float acceleration, float maxSpeed)
    {
        playerMovement.velocity = new float3(playerMovement.velocity.x, math.max(playerMovement.velocity.y, math.sqrt(2 * height * math.distance(float3.zero, playerMovement.settings.gravity))), playerMovement.velocity.z);
        if(!movement.Equals(float3.zero))
            playerMovement.velocity = ClampedAccelerate(math.normalize(movement), playerMovement.velocity, math.distance(movement, float3.zero) * acceleration, maxSpeed);
    }
}
*/