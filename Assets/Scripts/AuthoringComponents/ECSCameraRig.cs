using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
/*
[RequiresEntityConversion]
public class ECSCameraRig : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(head);
        referencedPrefabs.Add(leftHand);
        referencedPrefabs.Add(rightHand);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<CameraRigData>(entity);
        if (FindObjectOfType<CameraRig>())
        {
            Debug.Log("Linked Camera Rig");
            FindObjectOfType<CameraRig>().Link(entity);
        }
        else
        {
            Debug.LogError("Could not link camera rigs");
        }
    }
}
public class ECSCameraRigSystem : SystemBase
{
    protected override void OnCreate()
    {
        EntityManager entityManager = EntityManager;
        Entities.ForEach((Entity entity, ref CameraRigData cameraRig) => {
            cameraRig.head = entityManager.GetBuffer<Child>(entity)[0].Value;
            cameraRig.leftHand = entityManager.GetBuffer<Child>(entity)[1].Value;
            cameraRig.rightHand = entityManager.GetBuffer<Child>(entity)[2].Value;
        }).Run();
    }
    protected override void OnUpdate()
    {
        
    }
}
*/