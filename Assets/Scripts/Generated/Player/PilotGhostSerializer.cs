using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Transforms;

public struct PilotGhostSerializer : IGhostSerializer<PilotSnapshotData>
{
    private ComponentType componentTypeCameraRigChild;
    private ComponentType componentTypePilotData;
    private ComponentType componentTypePilotMovementSystemData;
    private ComponentType componentTypePilotSetupRequired;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<PilotData> ghostPilotDataType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<PilotMovementSystemData> ghostPilotMovementSystemDataType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 2;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<PilotSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeCameraRigChild = ComponentType.ReadWrite<CameraRigChild>();
        componentTypePilotData = ComponentType.ReadWrite<PilotData>();
        componentTypePilotMovementSystemData = ComponentType.ReadWrite<PilotMovementSystemData>();
        componentTypePilotSetupRequired = ComponentType.ReadWrite<PilotSetupRequired>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostPilotDataType = system.GetArchetypeChunkComponentType<PilotData>(true);
        ghostPilotMovementSystemDataType = system.GetArchetypeChunkComponentType<PilotMovementSystemData>(true);
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>(true);
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref PilotSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataPilotData = chunk.GetNativeArray(ghostPilotDataType);
        var chunkDataPilotMovementSystemData = chunk.GetNativeArray(ghostPilotMovementSystemDataType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetPilotDataPlayerId(chunkDataPilotData[ent].PlayerId, serializerState);
        snapshot.SetPilotMovementSystemDatavelocity(chunkDataPilotMovementSystemData[ent].velocity, serializerState);
        snapshot.SetPilotMovementSystemDataonGround(chunkDataPilotMovementSystemData[ent].onGround, serializerState);
        snapshot.SetPilotMovementSystemDatajumpCooldown(chunkDataPilotMovementSystemData[ent].jumpCooldown, serializerState);
        snapshot.SetPilotMovementSystemDatamovementMode(chunkDataPilotMovementSystemData[ent].movementMode, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
