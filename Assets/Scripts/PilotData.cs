using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct PilotData : IComponentData
{
    [GhostDefaultField]
    public int PlayerId;
    public bool clientHasAuthority;
}
