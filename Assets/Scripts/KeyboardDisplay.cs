using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardDisplay : MonoBehaviour
{
    public static string dispaly = "192.168.7.94";
    public static bool enterPressed;
    public TextMesh text;
    public GameObject[] destroyOnEnter;
    public static List<Entity> entityDestroyOnEnter = new List<Entity>();

    public void Update()
    {
        text.text = dispaly.ToString();
        if (enterPressed)
        {
            foreach(GameObject go in destroyOnEnter)
            {
                Destroy(go);
            }
            Destroy(gameObject);
            Destroy(text.gameObject);
            foreach (World world in World.All)
            {
                if (world.GetExistingSystem<ClientSimulationSystemGroup>() == null)
                    continue;
                foreach (Entity e in entityDestroyOnEnter)
                {
                    DestroyEntityAndChildren(e, world.EntityManager);
                }

            }
        }
    }
    private void DestroyEntityAndChildren(Entity entity, EntityManager entityManager)
    {
        if (entityManager.HasComponent<Child>(entity))
        {
            var child = entityManager.GetBuffer<Child>(entity).ToNativeArray(Allocator.Temp);
            for (int i = 0; i < child.Length; i++)
            {
                DestroyEntityAndChildren(child[i].Value, entityManager);
            }
            child.Dispose();
        }
        entityManager.DestroyEntity(entity);
    }
}
