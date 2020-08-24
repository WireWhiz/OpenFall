using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.XR;

public struct PilotInput : ICommandData<PilotInput>
{
    public uint Tick => tick;
    public uint tick;

    public float2 movement;
    public bool jumping;

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    [System.Serializable]
    public struct Transform
    {
        public float3 position;
        public quaternion rotation;
        public void Deserialize(ref DataStreamReader reader, int quantizatinon)
        {
            position.x = reader.ReadInt() / (float)quantizatinon;
            position.y = reader.ReadInt() / (float)quantizatinon;
            position.z = reader.ReadInt() / (float)quantizatinon;
            rotation.value.x = reader.ReadInt() / (float)quantizatinon;
            rotation.value.y = reader.ReadInt() / (float)quantizatinon;
            rotation.value.z = reader.ReadInt() / (float)quantizatinon;
            rotation.value.w = reader.ReadInt() / (float)quantizatinon;
        }
        public void Serialize(ref DataStreamWriter writer, int quantizatinon)
        {
            writer.WriteInt((int)(position.x * quantizatinon));
            writer.WriteInt((int)(position.y * quantizatinon));
            writer.WriteInt((int)(position.z * quantizatinon));
            writer.WriteInt((int)(rotation.value.x * quantizatinon));
            writer.WriteInt((int)(rotation.value.y * quantizatinon));
            writer.WriteInt((int)(rotation.value.z * quantizatinon));
            writer.WriteInt((int)(rotation.value.w * quantizatinon));
        }
    }
    
    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;

        movement.x = reader.ReadInt() / 100f;
        movement.y = reader.ReadInt() / 100f;
        jumping = (reader.ReadInt() > 0);

        head.Deserialize(ref reader, 10000);
        leftHand.Deserialize(ref reader, 1000);
        rightHand.Deserialize(ref reader, 1000);
    } 
    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt((int)(movement.x * 100));
        writer.WriteInt((int)(movement.y * 100));
        writer.WriteInt(jumping ? 1 : 0);

        head.Serialize(ref writer, 10000);
        leftHand.Serialize(ref writer, 1000);
        rightHand.Serialize(ref writer, 1000);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, PilotInput baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    
    public void Serialize(ref DataStreamWriter writer, PilotInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class PilotSendCommandSystem : CommandSendSystem<PilotInput>
{
}

public class PilotReceiveCommandSystem : CommandReceiveSystem<PilotInput>
{
}

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class PilotInputNetowrking : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnablePlayersGhostReceiveSystemComponent>();
    }
    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        var entityManager = EntityManager;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithStructuralChanges().WithNone<PilotInput>().ForEach((Entity ent, ref PilotData pilot ) =>
            {
                if (pilot.PlayerId == localPlayerId)
                {
                    UnityEngine.Debug.Log("Adding input buffer");
                    pilot.clientHasAuthority = true;
                    entityManager.AddBuffer<PilotInput>(ent);
                    entityManager.SetComponentData(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            }).Run();
            
            return;
        }
        //Debug.Log("updateing input");
        var input = default(PilotInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        var heads = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.Head, heads);
        var leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        var rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        InputDevice head = new InputDevice();
        if (heads.Count == 1)
        {
            head = heads[0];
        }

        InputDevice leftHand = new InputDevice();
        if (leftHandDevices.Count == 1)
        {
            leftHand = leftHandDevices[0];
        }

        InputDevice rightHand = new InputDevice();
        if (rightHandDevices.Count == 1)
        {
            rightHand = rightHandDevices[0];
        }

        head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPosition);
        head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headRotation);
        leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition);
        leftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation);
        rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition);
        rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation);
        input.movement = InputManager.pilotInput.movement;
        input.jumping = InputManager.pilotInput.jumping;
        input.head = new PilotInput.Transform
        {
            position = headPosition,
            rotation = headRotation
        };
        input.leftHand = new PilotInput.Transform
        {
            position = leftPosition,
            rotation = leftRotation
        };
        input.rightHand = new PilotInput.Transform
        {
            position = rightPosition,
            rotation = rightRotation
        };
        var inputBuffer = EntityManager.GetBuffer<PilotInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class CameraRigSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        bool isFinal = group.IsFinalPredictionTick;

        var deltaTime = Time.DeltaTime;
        var entityManager = EntityManager;

        Vector3 headPose = default;
        Quaternion headRot = default;
        Vector3 leftPose = default;
        Quaternion leftRot = default;
        Vector3 rightPose = default;
        Quaternion rightRot = default;
        if (isFinal && World.GetExistingSystem<ClientSimulationSystemGroup>() != null)//if this is the tick that we are showing to the player and this is the player then update to the latest values
        {
            var heads = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, heads);
            var leftHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            var rightHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

            InputDevice head = new InputDevice();
            if (heads.Count == 1)
            {
                head = heads[0];
            }
            Vector3 position;
            Quaternion rotation;
            head.TryGetFeatureValue(CommonUsages.devicePosition, out headPose);
            head.TryGetFeatureValue(CommonUsages.deviceRotation, out headRot);

            InputDevice leftHand = new InputDevice();
            if (leftHandDevices.Count == 1)
            {
                leftHand = leftHandDevices[0];
            }
            leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out leftPose);
            leftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out leftRot);

            InputDevice rightHand = new InputDevice();
            if (rightHandDevices.Count == 1)
            {
                rightHand = rightHandDevices[0];
            }
            rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out rightPose);
            rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out rightRot);

        }

        Entities.ForEach((Entity ent, DynamicBuffer<PilotInput> inputBuffer, ref PredictedGhostComponent prediction, in PilotData pilot, in CameraRigChild cameraRigChild) =>
        {
            if (cameraRigChild.Value == Entity.Null || !GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            inputBuffer.GetDataAtTick(tick, out PilotInput input);

            if (pilot.clientHasAuthority && isFinal)//if this is the tick that we are showing to the player and this is the player then update to the latest values
            {
                input.head.position = headPose;
                input.head.rotation = headRot;
                input.leftHand.position = leftPose;
                input.leftHand.rotation = leftRot;
                input.rightHand.position = rightPose;
                input.rightHand.rotation = rightRot;

            }
            
            var cameraRig = entityManager.GetComponentData<CameraRigData>(cameraRigChild.Value);
            entityManager.SetComponentData(cameraRig.head, new Translation { Value = input.head.position });
            entityManager.SetComponentData(cameraRig.head, new Rotation { Value = input.head.rotation });
            entityManager.SetComponentData(cameraRig.leftHand, new Translation { Value = input.leftHand.position });
            entityManager.SetComponentData(cameraRig.leftHand, new Rotation { Value = input.leftHand.rotation });
            entityManager.SetComponentData(cameraRig.rightHand, new Translation { Value = input.rightHand.position });
            entityManager.SetComponentData(cameraRig.rightHand, new Rotation { Value = input.rightHand.rotation });

        }).Run();

    }
}
