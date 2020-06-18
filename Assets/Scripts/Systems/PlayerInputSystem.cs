using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateBefore(typeof(PlayerMovementSystem))]
public class PlayerInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float2 movement = GameController.pilotInput.Movement;
        bool jumping = GameController.pilotInput.Jump;
        JobHandle inputJob = Entities.ForEach((ref PlayerInputData input)=>
        {
            input.movement = movement;
            input.jumping = jumping;
        }).Schedule(inputDeps);
        inputJob.Complete();

        return inputDeps;
    }
}
