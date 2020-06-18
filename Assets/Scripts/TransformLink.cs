using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

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
        switch (syncType)
        {
            case SyncType.EntityToThis:
                transform.position = entityManager.GetComponentData<Translation>(target).Value;
                transform.rotation = entityManager.GetComponentData<Rotation>(target).Value;
                break;
            case SyncType.ThisToEntityLocal:
                entityManager.SetComponentData<Translation>(target, new Translation() { Value = transform.localPosition });
                entityManager.SetComponentData<Rotation>(target, new Rotation() { Value = transform.localRotation });
                break;
        }
    }
    private void LateUpdate()
    {
        switch (syncType)
        {
            case SyncType.EntityToThis:
                transform.position = entityManager.GetComponentData<Translation>(target).Value;
                transform.rotation = entityManager.GetComponentData<Rotation>(target).Value;
                break;
            case SyncType.ThisToEntityLocal:
                entityManager.SetComponentData<Translation>(target, new Translation() { Value = transform.localPosition });
                entityManager.SetComponentData<Rotation>(target, new Rotation() { Value = transform.localRotation });
                break;
        }
    }
    public enum SyncType
    {
        EntityToThis,
        ThisToEntityLocal
    }
}
