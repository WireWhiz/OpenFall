using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct UIKeyboardButton : IComponentData
{
    public char key;
}
[UpdateInGroup(typeof(ClientSimulationSystemGroup)), UpdateAfter(typeof(UIRaycastSystem))]
public class UIKeyboardDisplaySystem : SystemBase
{
   
    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        NativeList<char> keys = new NativeList<char>(Allocator.Temp);
        Entities.WithStructuralChanges().WithAll<UIButtonPress>().ForEach((Entity entity, in UIKeyboardButton button) =>
        {
            keys.Add(button.key);
            entityManager.RemoveComponent<UIButtonPress>(entity);
        }).Run();
        foreach (char key in keys)
        {
            switch (key)
            {
                case '<':
                    KeyboardDisplay.dispaly = KeyboardDisplay.dispaly.Remove(KeyboardDisplay.dispaly.Length-1, 1);
                    UnityEngine.Debug.Log("backspace");
                    break;
                case 'e':
                    var singleton = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(typeof(IPConnection));
                    World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(singleton, new IPConnection { Value = KeyboardDisplay.dispaly });
                    UnityEngine.Debug.Log("enter");
                    KeyboardDisplay.enterPressed = true;
                    break;
                default:
                    KeyboardDisplay.dispaly += key;
                    UnityEngine.Debug.Log(key);
                    break;
            }
        }
        keys.Dispose();
    }
}
public struct IPConnection : IComponentData
{
    public NativeString32 Value;
}