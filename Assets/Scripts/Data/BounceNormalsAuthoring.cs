using UnityEngine;
using Unity.Entities;

public class BounceNormalsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        entityManager.AddBuffer<BounceNormals>(entity);
    }
}
