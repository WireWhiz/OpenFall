using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class TransformLink : MonoBehaviour
{
    public Entity target;
    public SyncType syncType;

    private EntityManager entityManager;
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    private void Update()
    {
        Sync();
    }
    private void LateUpdate()
    {
        Sync();
    }
    private void Sync()
    {
        switch (syncType)
        {
            case SyncType.EntityToThis:
                transform.position = entityManager.GetComponentData<LocalToWorld>(target).Position;
                transform.rotation = entityManager.GetComponentData<LocalToWorld>(target).Rotation;
                break;
            case SyncType.ThisToEntityLocal:
                entityManager.SetComponentData(target, new Translation() { Value = transform.localPosition });
                entityManager.SetComponentData(target, new Rotation() { Value = transform.localRotation });
                break;
        }
    }
    public enum SyncType
    {
        EntityToThis,
        ThisToEntityLocal
    }
}
