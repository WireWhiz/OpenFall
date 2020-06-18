using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerMovementData : IComponentData
{
    public float movementSpeed;
    public float3 gravity;
    public float3 velocity;
}
