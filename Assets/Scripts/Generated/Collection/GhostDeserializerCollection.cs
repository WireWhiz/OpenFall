using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct PlayersGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "PilotGhostSerializer",
        };
        return arr;
    }

    public int Length => 1;
#endif
    public void Initialize(World world)
    {
        var curPilotGhostSpawnSystem = world.GetOrCreateSystem<PilotGhostSpawnSystem>();
        m_PilotSnapshotDataNewGhostIds = curPilotGhostSpawnSystem.NewGhostIds;
        m_PilotSnapshotDataNewGhosts = curPilotGhostSpawnSystem.NewGhosts;
        curPilotGhostSpawnSystem.GhostType = 0;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_PilotSnapshotDataFromEntity = system.GetBufferFromEntity<PilotSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<PlayersGhostDeserializerCollection>.InvokeDeserialize(m_PilotSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_PilotSnapshotDataNewGhostIds.Add(ghostId);
                m_PilotSnapshotDataNewGhosts.Add(GhostReceiveSystem<PlayersGhostDeserializerCollection>.InvokeSpawn<PilotSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<PilotSnapshotData> m_PilotSnapshotDataFromEntity;
    private NativeList<int> m_PilotSnapshotDataNewGhostIds;
    private NativeList<PilotSnapshotData> m_PilotSnapshotDataNewGhosts;
}
public struct EnablePlayersGhostReceiveSystemComponent : IComponentData
{}
public class PlayersGhostReceiveSystem : GhostReceiveSystem<PlayersGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnablePlayersGhostReceiveSystemComponent>();
    }
}
