using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

[GenerateAuthoringComponent]
public struct TransformInputData : IComponentData
{
    public InputSource inputSource;
}
public enum InputSource
{
    head,
    leftHand,
    rightHand
}
