using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class HeadCamera : MonoBehaviour
{
    private Camera cam;
    public void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = false;
        XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRInputSubsystem>().TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
        Application.onBeforeRender += () => {
            cam.enabled = true;
            var heads = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.Head, heads);
            InputDevice head = new InputDevice();
            if (heads.Count == 1)
            {
                head = heads[0];
            }
            head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPosition);
            head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headRotation);
            
            
            var ent = CameraRigEntity.Value;
            if(ent != Entity.Null)
            {
                var entMan = CameraRigEntity.World.EntityManager;
                transform.position = entMan.GetComponentData<LocalToWorld>(ent).Position + (float3)headPosition;
                transform.rotation = headRotation * entMan.GetComponentData<LocalToWorld>(ent).Rotation;
            }
            else
            {
                transform.position = headPosition;
                transform.rotation = headRotation;
            }
        };
    }

}

public static class CameraRigEntity
{
    public static Entity Value;
    public static World World;
}