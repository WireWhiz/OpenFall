
using Newtonsoft.Json;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class Game : SystemBase
{
    // Singleton component to trigger connections once from a control system
    struct InitGameComponent : IComponentData
    {
    }
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitGameComponent>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate()
    {
        // Destroy singleton to prevent system from running again
        foreach (var world in World.All)
        {
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                if(!HasSingleton<IPConnection>())
                return;
            }
        }
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        foreach (var world in World.All)
        {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                Debug.Log("Client trying to connect to: " + GetSingleton<IPConnection>().Value.ToString());
                NetworkEndPoint.TryParse(GetSingleton<IPConnection>().Value.ToString(), 7979, out NetworkEndPoint ep);
                network.Connect(ep);
            }
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                // Server world automatically listens for connections from any host
                Debug.Log("Server listening for connections");
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
                ep.Port = 7979;
                network.Listen(ep);
            }
        }
    }
}

