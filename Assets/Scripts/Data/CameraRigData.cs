using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct CameraRigData : IComponentData
{
    public Entity head;
    public Entity leftHand;
    public Entity rightHand;
}
