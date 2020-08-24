using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections;

public static class CharacterController
{
    public static unsafe void Move(CharacterControllerInput characterController, ref NativeList<float3> bounceNormals, in CollisionWorld collisionWorld, out CharacterControllerOutput output) 
    {
        //Create delta
        float3 delta = characterController.moveDelta;
        output = default;

        output.position = characterController.position;

        if (delta.Equals(float3.zero))
            return;
        CheckForClipping(ref delta, ref output.position, ref bounceNormals, characterController, collisionWorld);
        //move controller
        output.position += delta;

        //Solve collision
        ColliderDistanceInput collisionCheck = new ColliderDistanceInput()
        {
            Collider = (Collider*)characterController.capsuleCollider.GetUnsafePtr(),
            MaxDistance = 0,
            Transform = new RigidTransform(quaternion.identity, output.position)
        };

        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
        if (collisionWorld.CalculateDistance(collisionCheck, ref hits))
        {
            var bounces = GetBounces(hits); //get the normal of all the hits
            if (CheckForWalls(bounces, characterController.maxAngle)) // use sliding or snapping
            {
                for(int i = 0; i < bounces.Length; i++)
                    bounceNormals.Add(bounces[i]);//add bounces
                output.position -= GetBounceNormal(bounces);//get a combination of all the hits
            }
            else
            {
                SnapToGround(hits, ref output.position, ref characterController);
                bounceNormals.Add(new float3(0, -1, 0));//add a single bounce since we are on the ground;
            }

        }
        output.onGround = GetGrounded(bounceNormals, ref characterController);
        hits.Dispose();
    }
    private static unsafe bool CheckForClipping(ref float3 delta, ref float3 position, ref NativeList<float3> bounces, in CharacterControllerInput characterController, in CollisionWorld collisionWorld)
    {
        var geomitry = new SphereGeometry() //create collider
        {
            Radius = 0.01f
        };
        BlobAssetReference<Collider> collider = SphereCollider.Create( geomitry, characterController.filter);

        var bodyCheckInput = new ColliderCastInput() //set up cast varibles
        {
            Collider = (Collider*)collider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = position + characterController.center,//we want to cast from the center of the capsuel 
            End = position + characterController.center + delta
        };

        if (collisionWorld.CastCollider(bodyCheckInput, out ColliderCastHit bodyHit)) //if we hit anything
        {
            bounces.Add(bodyHit.SurfaceNormal);//add to bounces
            delta *= bodyHit.Fraction;//set delta to the length of the raycast so we move directly to the hit point
            return true;
        }
        return false;
    }
    private static void SnapToGround(NativeList<DistanceHit> hits, ref float3 position, ref CharacterControllerInput characterController)
    {
        float maxHeight = 0;
        for (int i = 0; i < hits.Length; i++)//find the higest point
        {
            float3 delta = hits[i].Position - (position + characterController.footOffset);

            //we need to correct for the curve of the capsule
            float angle = math.acos(math.distance(delta.ProjectOnPlane(new float3(0, 1, 0)), float3.zero) / characterController.raduis);
            float offset = characterController.raduis - math.sin(angle) * characterController.raduis;

            //find the higest corrected float;
            if (delta.y - offset > maxHeight)
            {
                maxHeight = delta.y - offset;
            }
        }
        position += new float3(0, maxHeight, 0);//set our transform to the higest point.
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
    private static bool GetGrounded(in NativeList<float3> bounceNormals, ref CharacterControllerInput characterController)
    {
        for (int i = 0; i < bounceNormals.Length; i++)
        {
            if (bounceNormals[i].AngleFrom(new float3(0, -1, 0)) < math.radians(characterController.maxAngle)) //checks bounces to see if any were the ground
            {
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
    public static float3 GetGroundNormal(float3 footOfset, float distance, CollisionFilter Filter, CollisionWorld collisionWorld)
    {
        float3 normal = new float3(0, 1, 0);
        RaycastInput input = new RaycastInput()
        {
            Start = footOfset,
            End = footOfset - new float3(0, distance, 0),
            Filter = Filter
        };
        if (collisionWorld.CastRay(input, out RaycastHit hit))
        {
            normal = hit.SurfaceNormal;
            UnityEngine.Debug.Log("detected ground");
        }
        return normal;
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

public struct CharacterControllerInput
{
    public float raduis;
    public float height;
    public float maxAngle;
    public float3 footOffset;
    public float3 position;
    public float3 moveDelta;
    public UnityEngine.LayerMask layersToIgnore;
    public float3 center => footOffset + new float3(0, height / 2, 0);
    public float3 vertexTop => footOffset + new float3(0, height - raduis, 0);
    public float3 vertexBottom => footOffset + new float3(0, raduis, 0);
    public float3 top => footOffset + new float3(0, height, 0);
    public BlobAssetReference<Collider> capsuleCollider => CapsuleCollider.Create(new CapsuleGeometry() { Radius = raduis, Vertex0 = vertexTop, Vertex1 = vertexBottom }, filter);
    public CollisionFilter filter
    {
        get
        {
            return new CollisionFilter()
            {
                BelongsTo = (uint)(~layersToIgnore.value),
                CollidesWith = (uint)(~layersToIgnore.value)
            };
        }
    }
}

public struct CharacterControllerOutput
{
    public float3 position;
    public bool onGround;
}
