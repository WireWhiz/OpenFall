using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections;

public class CharacterControllerSystem : SystemBase
{
    protected override void OnUpdate()
    {

        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();//get references before we can no longer access the main thread
        CollisionWorld collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        EntityManager entityManager = EntityManager;
        //set collisionWorld to readOnly with .WithReadOnly() so it does not throw errors
        //JobHandle controllerJob = 
        Entities.WithReadOnly(collisionWorld).ForEach((ref CharacterControllerData characterController, ref Translation translation, ref DynamicBuffer<BounceNormals> bounceNormalsBuffer) =>
        {
            bounceNormalsBuffer.Clear();//reset buffer and convert to usable state
            DynamicBuffer<float3> bounceNormals = bounceNormalsBuffer.Reinterpret<float3>();

            Move(ref characterController, ref translation, ref bounceNormals, collisionWorld);//move 
            characterController.onGround = GetGrounded(bounceNormals, ref characterController);//update on ground

            characterController.moveDelta = float3.zero;//reset moveDelta
        }).Run();//.ScheduleParallel(JobHandle.CombineDependencies(Dependency, physicsWorldSystem.GetOutputDependency()));//run on multiple cores
        //controllerJob.Complete();
    }
    

    private static unsafe void Move(ref CharacterControllerData characterController, ref Translation translation, ref DynamicBuffer<float3> bounceNormals, in CollisionWorld collisionWorld) {
        //Create Collider
        var filter = characterController.Filter;
        CapsuleGeometry capsuleGeometry = new CapsuleGeometry() //create our collider
        {
            Vertex0 = characterController.vertexTop,
            Vertex1 = characterController.vertexBottom,
            Radius = characterController.raduis
        };
        BlobAssetReference<Collider> capsuleCollider = CapsuleCollider.Create(capsuleGeometry, filter);
        //Create delta
        float3 delta = characterController.moveDelta;

        //constrain by near objects
        ColliderDistanceInput constrainCheck = new ColliderDistanceInput()
        {
            Collider = (Collider*)capsuleCollider.GetUnsafePtr(),
            MaxDistance = characterController.skin,
            Transform = new RigidTransform(quaternion.identity, translation.Value)
        };
        var hits = new NativeList<DistanceHit>(Allocator.Temp); 
        if (collisionWorld.CalculateDistance(constrainCheck, ref hits))
        {
            for(int i = 0; i < hits.Length; i++)
           {
                if (math.dot(delta, hits[i].SurfaceNormal) < 0)
                {
                    //bounceNormals.Add(hits[i].SurfaceNormal); 
                    //delta = delta.ProjectOnPlane(hits[i].SurfaceNormal);
                        
                }
            }
        }

        CheckForClipping(ref delta, ref translation, ref bounceNormals, characterController, collisionWorld);
        //move controller
        translation.Value += delta;

        //Solve collision
        ColliderDistanceInput collisionCheck = new ColliderDistanceInput()
        {
            Collider = (Collider*)capsuleCollider.GetUnsafePtr(),
            MaxDistance = 0,
            Transform = new RigidTransform(quaternion.identity, translation.Value)
        };

        hits.Clear();//clear hits so we can use it again
        if (collisionWorld.CalculateDistance(collisionCheck, ref hits))
        {
            var bounces = GetBounces(hits); //get the normal of all the hits
            if (CheckForWalls(bounces, characterController.maxAngle)) // use sliding or snapping
            {
                for(int i = 0; i < bounces.Length; i++)
                    bounceNormals.Add(bounces[i]);//add bounces
                translation.Value -= GetBounceNormal(bounces);//get a combination of all the hits
            }
            else
            {
                SnapToGround(hits, ref translation, ref characterController);
                bounceNormals.Add(new float3(0, -1, 0));//add a single bounce since we are on the ground;
            }

        }

        hits.Dispose();
    }
    private static unsafe bool CheckForClipping(ref float3 delta, ref Translation translation, ref DynamicBuffer<float3> bounces, in CharacterControllerData characterController, in CollisionWorld collisionWorld)
    {
        var geomitry = new SphereGeometry() //create collider
        {
            Radius = 0.01f
        };
        BlobAssetReference<Collider> collider = SphereCollider.Create( geomitry, characterController.Filter);

        var bodyCheckInput = new ColliderCastInput() //set up cast varibles
        {
            Collider = (Collider*)collider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = translation.Value + characterController.center,//we want to cast from the center of the capsuel 
            End = translation.Value + characterController.center + delta
        };

        if (collisionWorld.CastCollider(bodyCheckInput, out ColliderCastHit bodyHit)) //if we hit anything
        {
            bounces.Add(bodyHit.SurfaceNormal);//add to bounces
            delta *= bodyHit.Fraction;//set delta to the length of the raycast so we move directly to the hit point
            return true;
        }
        return false;
    }
    private static void SnapToGround(NativeList<DistanceHit> hits, ref Translation translation, ref CharacterControllerData characterController)
    {
        float maxHeight = 0;
        for (int i = 0; i < hits.Length; i++)//find the higest point
        {
            float3 delta = hits[i].Position - (translation.Value + characterController.footOffset);

            //we need to correct for the curve of the capsule
            float angle = math.acos(math.distance(delta.ProjectOnPlane(new float3(0, 1, 0)), float3.zero) / characterController.raduis);
            float offset = characterController.raduis - math.sin(angle) * characterController.raduis;

            //find the higest corrected float;
            if (delta.y - offset > maxHeight)
            {
                maxHeight = delta.y - offset;
            }
        }
        translation.Value += new float3(0, maxHeight, 0);//set our transform to the higest point.
    }
    private static float3 GetBounceNormal(NativeArray<float3> bounces)
    {
        float3 maxDists = float3.zero;
        for(int i = 0; i < bounces.Length; i++) //combine all of the bounces to get the vector we need
        {
            float3 vector = bounces[i];
            maxDists = new float3()
            {
                x = (math.abs(maxDists.x) < math.abs(vector.x)) ? vector.x : maxDists.x,
                y = (math.abs(maxDists.y) < math.abs(vector.y)) ? vector.y : maxDists.y,
                z = (math.abs(maxDists.z) < math.abs(vector.z)) ? vector.z : maxDists.z
            };
        }
        return maxDists;
    }
    private static bool GetGrounded(in DynamicBuffer<float3> bounceNormals, ref CharacterControllerData characterController)
    {
        characterController.groundNormal = new float3(0, 1, 0);
        for (int i = 0; i < bounceNormals.Length; i++)
        {
            if (bounceNormals[i].AngleFrom(new float3(0, -1, 0)) < math.radians(characterController.maxAngle)) //checks bounces to see if any were the ground
            {
                characterController.groundNormal = bounceNormals[i];
                return true;
            }
                
        }
        return false;
    }
    private static NativeList<float3> GetBounces(NativeList<DistanceHit> hits)
    {
        var bounces = new NativeList<float3>(Allocator.Temp);
        for (int i = 0; i < hits.Length; i++)
        {
            bounces.Add(hits[i].SurfaceNormal * hits[i].Distance);//get all the normals from the hits
        }
        return bounces;
    }
    private static bool CheckForWalls(NativeArray<float3> bounces, float maxAngle)
    {
        for (int i = 0; i < bounces.Length; i++)
        {
            if (bounces[i].AngleFrom(new float3(0, -1, 0)) > math.radians(maxAngle))//are any of these a slope or wall? if so return true
                return true;
        }
        return false;
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
    public static float AngleFrom(this float3 vector1, float3 vector2)
    {
        return math.acos(math.dot(math.normalize(vector1), math.normalize(vector2)));
    }
    public static quaternion ProjectOnPlane(this quaternion rotation, float3 vector)
    {
        float3 flatRotatedVector = math.mul(rotation, new float3(0, 0, 1)).ProjectOnPlane(vector);
        float angle = 0;
        if (math.dot(flatRotatedVector, new float3(1, 0, 0)) > 0)
            angle = new float3(0, 0, 1).AngleFrom(flatRotatedVector);
        else
            angle = 2 * math.PI - new float3(0, 0, 1).AngleFrom(flatRotatedVector);
        if (!float.IsNaN(angle))
            return quaternion.AxisAngle(vector, angle);
        else
            return quaternion.identity;
    }

}