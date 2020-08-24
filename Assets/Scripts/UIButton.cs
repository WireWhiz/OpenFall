using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[GenerateAuthoringComponent]
public struct UIButton : IComponentData
{

}
public struct UIButtonPress : IComponentData
{
    public RaycastHit hit;
}

public struct KeyPressData : IComponentData
{
    public char Value;  
}