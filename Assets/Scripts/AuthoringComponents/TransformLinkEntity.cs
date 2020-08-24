using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class TransformLinkEntity : MonoBehaviour, IConvertGameObjectToEntity
{
    public TransformLinkTarget target;

    public enum TransformLinkTarget
    {
        root,
        head,
        leftHand,
        rightHand
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (!FindObjectOfType<CameraRig>())
            return;
        CameraRig cameraRig = FindObjectOfType<CameraRig>().GetComponent<CameraRig>();
        switch (target)
        {
            case TransformLinkTarget.root:
                cameraRig.GetComponent<TransformLink>().target = entity;
                cameraRig.GetComponent<TransformLink>().entityManager = dstManager;
                break;
            case TransformLinkTarget.head:
                cameraRig.head.GetComponent<TransformLink>().target = entity;
                cameraRig.head.GetComponent<TransformLink>().entityManager = dstManager;
                break;
            case TransformLinkTarget.leftHand:
                cameraRig.leftHand.GetComponent<TransformLink>().target = entity;
                cameraRig.leftHand.GetComponent<TransformLink>().entityManager = dstManager;
                break;
            case TransformLinkTarget.rightHand:
                cameraRig.rightHand.GetComponent<TransformLink>().target = entity;
                cameraRig.rightHand.GetComponent<TransformLink>().entityManager = dstManager;
                break;
        }
        Debug.Log(entity);
    }
}
