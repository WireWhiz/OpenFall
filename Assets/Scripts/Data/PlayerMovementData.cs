using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerMovementData : IComponentData
{
    public Settings settings;
    public ControlType controlType;
    public float3 velocity;
    public float wallTime;
    public float3 lastWallNormal;
    public bool wallrunning;
    public bool jumping;
    public bool secondJumping;
    public bool jumpCooldown;
    public enum ControlType
    {
        localToHead,
        localToHand
    }
    [System.Serializable]
    public struct Settings
    {
        public float3 gravity;
        public float maxSpeed;
        public float acceleration;
        public float airAcceleration;
        public float wallAcceleration;
        public float jumpAcceleration;
        public float jumpHeight;
        public float secondJumpHeight;
        public float groundFriction;
        public float groundingDistance;
        public float runActivationHeadAngle;
        public float maxWallAngle;
        public float maxCornerAngle;
        public float maxWallTime;
        public float maxWallSpeed;
        public float wallRunningFriction;
        public float wallSnappingDistance;
    }
}
