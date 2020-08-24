using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DestroyOnEnter : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        KeyboardDisplay.entityDestroyOnEnter.Add(entity);
    }
}
