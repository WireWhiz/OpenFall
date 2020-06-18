using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections;
using UnityEngine.InputSystem.LowLevel;

public class CharacterControllerSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
       
        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        EntityManager entityManager = EntityManager;
        Entities.ForEach(( ref CharacterControllerData characterController, ref Translation translation, ref DynamicBuffer<BounceNormals> bounceNormalsBuffer) =>
        {
            bounceNormalsBuffer.Clear();
            DynamicBuffer<float3> bounceNormals = bounceNormalsBuffer.Reinterpret<float3>();
            characterController.onGround = false;
            float3 startPose = translation.Value;
            CheckIntersection(ref characterController,ref translation, ref bounceNormals, collisionWorld);
            Move(ref characterController, ref translation, ref bounceNormals, collisionWorld);
            characterController.moveDelta = float3.zero;
            characterController.lastMoveDelta = translation.Value - startPose;
            //var lookup = GetArchetypeChunkBufferType<BounceNormals>();
            
            //UnityEngine.Debug.Log(characterController.onGround);
        }).Run();
        return inputDeps;
    }

    private static unsafe void CheckIntersection(ref CharacterControllerData characterController, ref Translation translation, ref DynamicBuffer<float3> bounceNormals, CollisionWorld collisionWorld)
    {
        var filter = characterController.Filter;
        var topCheckInput = new RaycastInput()
        {
            Start = characterController.top + translation.Value,
            End = characterController.top + translation.Value - new float3(0, (characterController.raduis - characterController.skin) * 2, 0),
            Filter = filter,
        };
        if (collisionWorld.CastRay(topCheckInput, out RaycastHit topHit))
        {
            characterController.onGround = true;
            bounceNormals.Add(new float3(0, 1, 0));
            translation.Value += new float3(0, characterController.height - (characterController.raduis - characterController.skin) * 2 * topHit.Fraction - (characterController.raduis - characterController.skin), 0);
            return;
        }

        SphereGeometry sphereGeometry = new SphereGeometry()
        {
            Radius = (characterController.raduis - characterController.skin)
        };
        BlobAssetReference<Collider> sphereCollider = SphereCollider.Create(sphereGeometry, filter);
        var bodyCheckInput = new ColliderCastInput()
        {
            Collider = (Collider*)sphereCollider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = characterController.vertexTop + translation.Value,
            End = characterController.vertexBottom + translation.Value
        };
        if(collisionWorld.CastCollider(bodyCheckInput, out ColliderCastHit bodyHit))
        {
            //UnityEngine.Debug.Log("capusle hit");
            characterController.onGround = true;
            bounceNormals.Add(new float3(0, 1, 0));
            translation.Value += new float3(0, (characterController.height - characterController.raduis) - (characterController.height - characterController.raduis * 2) * bodyHit.Fraction - characterController.raduis, 0);
        }

    }
    private static unsafe void Move(ref CharacterControllerData characterController, ref Translation translation, ref DynamicBuffer<float3> bounceNormals, CollisionWorld collisionWorld) {
        var filter = characterController.Filter;
        CapsuleGeometry capsuleGeometry = new CapsuleGeometry()
        {
            Vertex0 = characterController.vertexTop,
            Vertex1 = characterController.vertexBottom + new float3(0, characterController.skin, 0),
            Radius = characterController.raduis
        };
        BlobAssetReference<Collider> capsuleCollider = CapsuleCollider.Create(capsuleGeometry, filter);
        float3 delta = characterController.moveDelta;
        //UnityEngine.Debug.Log(delta);
        //for (int i = 0; i < characterController.lod; i++)
        //{
        float3 skinOffset = math.normalize(delta) * characterController.skin;
        var input = new ColliderCastInput()
        {
            Collider = (Collider*)capsuleCollider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = translation.Value - skinOffset,
            End = translation.Value + delta,
        };
        //UnityEngine.Debug.DrawLine(input.Start, input.End, UnityEngine.Color.green);
        //UnityEngine.Debug.DrawLine(translation.Value, translation.Value - skinOffset, UnityEngine.Color.red);
        ColliderDistanceInput collisionCheck = new ColliderDistanceInput()
        {
            Collider = input.Collider,
            MaxDistance = 0,
            Transform = new RigidTransform(quaternion.identity,translation.Value)
        };
        collisionWorld.CalculateDistance(collisionCheck);
        translation.Value += delta;
        if (collisionWorld.CalculateDistance(collisionCheck, out DistanceHit hit))
        {
                UnityEngine.Debug.DrawLine(hit.Position, hit.Position - hit.SurfaceNormal * hit.Distance, UnityEngine.Color.blue, 10f);
                bounceNormals.Add(hit.SurfaceNormal);
                translation.Value -= hit.SurfaceNormal * hit.Distance;
            //float3 moved = (input.End - input.Start) * hit.Fraction - skinOffset;
            //float3 remaining = (input.End - input.Start) * (1 - hit.Fraction) - skinOffset;
            //translation.Value += moved;
                
                
            //delta = remaining.ProjectOnPlane(CapsuleHitNormal(hit.Position, characterController.footOffset + translation.Value, characterController.height, characterController.raduis));
            //UnityEngine.Debug.Log(skinOffset);
            //UnityEngine.Debug.Log("Final Normal: " + CapsuleHitNormal(hit.Position, characterController.footOffset + translation.Value, characterController.height, characterController.raduis));
        }
                
        //}


    }
    private static float3 CapsuleHitNormal(float3 hitPoint, float3 colliderBottom, float height, float raduis)
    {
        float3 pivit = hitPoint.Project( new float3(0, 1, 0)) - colliderBottom.Project(new float3(0, 1, 0)) - new float3(0, height / 2, 0);
        //UnityEngine.Debug.Log("Pivit: " + pivit);
        float clampValue = (height / 2 - raduis) / math.length(pivit);
        //UnityEngine.Debug.Log("correction value " + clampValue);
        if (math.abs(clampValue) < 1)
        {
            if (0 < clampValue)
                pivit *= clampValue;
            else if (clampValue < 0)
                pivit *= -clampValue;
        }
        
        //UnityEngine.Debug.Log("Corrected pivit: " + pivit);
        pivit += colliderBottom + new float3(0, height / 2, 0);
        //UnityEngine.Debug.Log("World Space pivit: " + pivit);

        //UnityEngine.Debug.DrawLine(pivit, hitPoint, UnityEngine.Color.blue);
        return math.normalize(hitPoint - pivit);
    }
}

public static class MathExtention
{
    public static float3 Project(this float3 vector, float3 normal)
    {
        normal = math.normalize(normal);
        float dist = math.dot(vector, normal);
        float3 projected = dist * normal;
        return projected;
    }
    public static float3 ProjectOnPlane(this float3 vector, float3 normal)
    {
        float3 projected = vector - vector.Project(normal);
        return projected;
    }
    
}