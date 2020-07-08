using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerMovementData : IComponentData
{
    public float maxSpeed;
    public float acceletaion;
    public float airAcceleration;
    public float jumpHeight;
    public float groundFriction;
    public float groundingDistance;
    public ControlType controlType;
    public float3 gravity;
    public float3 velocity;
    public enum ControlType
    {
        localToHead,
        localToHand
    }
}
