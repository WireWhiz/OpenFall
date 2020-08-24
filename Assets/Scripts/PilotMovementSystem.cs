using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class PilotMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        var tick = group.PredictingTick;
        
        
        float deltaTime = Time.DeltaTime;
        var entityManager = EntityManager;

        var settings = ServerVariables.pilotSettings;
        Entities.WithReadOnly(collisionWorld).WithReadOnly(settings).ForEach((Entity ent, DynamicBuffer<PilotInput> inputBuffer, ref PredictedGhostComponent prediction, ref PilotMovementSystemData pilotMovement, ref Translation translation) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            
            inputBuffer.GetDataAtTick(tick, out PilotInput input);

            input.movement = input.movement.Quantize(100);
            input.head.position = input.head.position.Quantize(10000);
            input.head.rotation = input.head.rotation.Quantize(10000);
            input.movement = math.mul((input.head.rotation).ProjectOnPlane(new float3(0, 1, 0)), new float3(input.movement.x, 0, input.movement.y)).xz;
            float inputScale = math.distance(new float2(input.movement.x, input.movement.y), float2.zero);
            if (!input.jumping)
                pilotMovement.jumpCooldown = false;
            switch (pilotMovement.movementMode)
            {
                case PilotMovementSystemData.MovementMode.walking:
                    pilotMovement.velocity += new float3(0, -settings.speeds.gravity * deltaTime, 0);
                    float runWeight = math.dot(math.normalize(math.mul(input.head.rotation, new float3(0, 0, 1)).ProjectOnPlane(new float3(0, 1, 0))), math.normalize(new float3(input.movement.x, 0, input.movement.y)));

                    float movementSpeed = math.lerp(settings.speeds.maxGroundSpeed, settings.speeds.runSpeed, math.max(runWeight, 0));
                    float speed = math.distance(pilotMovement.velocity, float3.zero);
                    if (!input.movement.Equals(float2.zero))
                    {
                        if(speed < settings.speeds.minGroundSpeed)
                        {
                            float3 groundNormal = CharacterController.GetGroundNormal((input.head.position).ProjectOnPlane(new float3(0, 1, 0)) + translation.Value + new float3(0,0.5f,0), 1, pilotMovement.filter, collisionWorld);
                            //quaternion rotation = math.inverse(quaternion.LookRotationSafe(math.mul(quaternion.Euler(90, 0, 0), groundNormal), new float3(0, 1, 0)));
                            pilotMovement.velocity = (new float3(input.movement.x, 0, input.movement.y) * movementSpeed).ProjectOnPlane(groundNormal) + new float3(0, -0.2f, 0);
                        }
                        else
                            pilotMovement.velocity = ClampedAccelerate(input.movement, pilotMovement.velocity, settings.speeds.groundAcceleration * deltaTime, inputScale * movementSpeed);
                    }
                        

                    Move(ref pilotMovement, ref translation, input, settings, collisionWorld, deltaTime);

                    if (!pilotMovement.onGround)
                    {
                        pilotMovement.movementMode = PilotMovementSystemData.MovementMode.jumping;
                        break;
                    }

                    if (input.jumping && !pilotMovement.jumpCooldown)
                    {
                        pilotMovement.movementMode = PilotMovementSystemData.MovementMode.jumping;
                        Jump(ref pilotMovement, input.movement, settings.jumpHeight, settings.speeds.jumpAcceleration, settings.speeds.maxJumpSpeed, settings.speeds.gravity);
                        pilotMovement.jumpCooldown = true;
                        UnityEngine.Debug.Log("first jump");
                        break;
                    }

                    speed = math.distance(pilotMovement.velocity, float3.zero);
                    if (inputScale == 0 && speed < settings.speeds.minGroundSpeed)
                    {
                        pilotMovement.velocity = new float3(0, -0.2f, 0);
                    }
                    else
                    {
                        float drop = speed * settings.groundFriction * deltaTime;
                        pilotMovement.velocity *= math.max(speed - drop, 0) / speed;
                    }

                    break;

                case PilotMovementSystemData.MovementMode.jumping:
                    if(input.jumping && !pilotMovement.jumpCooldown)
                    {
                        Jump(ref pilotMovement, input.movement, settings.secondJumpHeight, settings.speeds.secondJumpAcceleration, settings.speeds.secondJumpMaxSpeed, settings.speeds.gravity);
                        pilotMovement.movementMode = PilotMovementSystemData.MovementMode.secondJumping;
                        pilotMovement.jumpCooldown = true;
                        UnityEngine.Debug.Log("second jump");
                    }

                    goto case PilotMovementSystemData.MovementMode.secondJumping;
                case PilotMovementSystemData.MovementMode.secondJumping:
                    pilotMovement.velocity += new float3(0, -settings.speeds.gravity * deltaTime, 0);
                    if (!input.movement.Equals(float2.zero))
                        pilotMovement.velocity = ClampedAccelerate(input.movement, pilotMovement.velocity, settings.speeds.airAcceleration * deltaTime, inputScale * settings.speeds.maxWallSpeed);
                    Move(ref pilotMovement, ref translation, input, settings, collisionWorld, deltaTime);
                    if (pilotMovement.onGround)
                    {
                        pilotMovement.movementMode = PilotMovementSystemData.MovementMode.walking;
                    }
                    break;
                case PilotMovementSystemData.MovementMode.sliding:
                    pilotMovement.velocity += new float3(0, -settings.speeds.gravity * deltaTime, 0);
                    break;
                case PilotMovementSystemData.MovementMode.wallRunning:
                    break;
            }
        }).Run();
    }
    private static void Move(ref PilotMovementSystemData pilotMovement, ref Translation translation, in PilotInput input, in ServerVariables.PilotSettings settings, CollisionWorld collisionWorld, float deltaTime)
    {
        var controllerInput = new CharacterControllerInput
        {
            position = translation.Value,
            footOffset = (input.head.position).ProjectOnPlane(new float3(0, 1, 0)),
            height = input.head.position.y + settings.headPadding,
            raduis = settings.playerRaduis,
            maxAngle = settings.maxWalkAngle,
            layersToIgnore = pilotMovement.layersToIgnore,
            moveDelta = pilotMovement.velocity * deltaTime
        };

        var hits = new NativeList<float3>(Allocator.Temp);

        CharacterController.Move(controllerInput, ref hits, collisionWorld, out CharacterControllerOutput output);
        translation.Value = output.position;
        for (int i = 0; i < hits.Length; i++)
        {
            pilotMovement.velocity = pilotMovement.velocity.ProjectOnPlane(hits[i]);
        }
        pilotMovement.onGround = output.onGround;

        hits.Dispose();
    }
    private static float3 ClampedAccelerate(float2 direction2, float3 currentVelocity, float acceleration, float maxSpeed)
    {
        float3 direction = math.normalize(new  float3(direction2.x, 0, direction2.y));
        float projectedSpeed = math.dot(currentVelocity.ProjectOnPlane(new float3(0,1,0)), direction);

        if (projectedSpeed + acceleration > maxSpeed)
            acceleration = math.max(maxSpeed - projectedSpeed, 0);

        return currentVelocity + acceleration * direction;
    }
    private static void Jump(ref PilotMovementSystemData pilotMovement, float2 movement, float height, float acceleration, float maxSpeed, float gravity)
    {
        pilotMovement.velocity = new float3(pilotMovement.velocity.x, math.max(pilotMovement.velocity.y, math.sqrt(2 * height * math.distance(float3.zero, gravity))), pilotMovement.velocity.z);
        if (!movement.Equals(float2.zero))
            pilotMovement.velocity = ClampedAccelerate(math.normalize(movement), pilotMovement.velocity, math.distance(movement, float2.zero) * acceleration, maxSpeed);
    }
}
