using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class TransformInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float3 newTranslation1 = CameraRig.instance.head.localPosition;
        quaternion newRotation1 = CameraRig.instance.head.localRotation;
        float3 newTranslation2 = CameraRig.instance.leftHand.localPosition;
        quaternion newRotation2 = CameraRig.instance.leftHand.localRotation;
        float3 newTranslation3 = CameraRig.instance.rightHand.localPosition;
        quaternion newRotation3 = CameraRig.instance.rightHand.localRotation;
        Entities.ForEach((ref Translation translation, ref Rotation rotation, in TransformInputData inputData) => {
            switch (inputData.inputSource)
            {
                case InputSource.head:
                    translation.Value = newTranslation1;
                    rotation.Value = newRotation1;
                    break;
                case InputSource.leftHand:
                    translation.Value = newTranslation2;
                    rotation.Value = newRotation2;
                    break;
                case InputSource.rightHand:
                    translation.Value = newTranslation3;
                    rotation.Value = newRotation3;
                    break;
            }
        }).Run();
    }
    
}
