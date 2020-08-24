using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.XR;

[GenerateAuthoringComponent]
public struct CameraRigData : IComponentData
{
    public Entity head;
    public Entity leftHand;
    public Entity rightHand;
}
public class NonNetworkCameraRigSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
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

        var pInput = InputManager.pilotInput;

        head.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 headPosition);
        head.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion headRotation);

        leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition);
        leftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation);
        rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition);
        rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation);
        
        Entities.ForEach((CameraRigData cameraRig) =>
        {

            SetComponent(cameraRig.head, new Translation { Value = headPosition });
            SetComponent(cameraRig.head, new Rotation { Value = headRotation });
            SetComponent(cameraRig.leftHand, new Translation { Value = leftPosition });
            SetComponent(cameraRig.leftHand, new Rotation { Value = leftRotation });
            SetComponent(cameraRig.rightHand, new Translation { Value = rightPosition });
            SetComponent(cameraRig.rightHand, new Rotation { Value = rightRotation });
        }).Run();
    }
}