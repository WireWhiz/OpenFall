using Unity.Entities;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public static CameraRig instance;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Entity linkedRig;
    private void Awake()
    {
        if(instance == null && instance != this)
        {
            Destroy(this);
        }
        instance = this;
    }

    public void Link(Entity root)
    {
        linkedRig = root;
    }
}
