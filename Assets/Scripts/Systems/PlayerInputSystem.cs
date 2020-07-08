using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateBefore(typeof(PlayerMovementSystem))]
public class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float2 movement = GameController.pilotInput.movement;
        bool jumping = GameController.pilotInput.Jump;
        Entities.ForEach((ref PlayerInputData input)=>
        {
            input.movement = movement;
            input.jumping = jumping;
        }).ScheduleParallel();

    }
}
