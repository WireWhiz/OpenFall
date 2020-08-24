using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public float deadzone;
    public InputActionAsset input;
    public static PilotInput pilotInput;

    void Start()
    {
        if (instance && instance != this)
        {
            Destroy(this);
        }
        instance = this;
        foreach (InputActionMap map in input.actionMaps)
        {
            map.Enable();
            switch (map.name)
            {
                case "Pilot":
                    map["Move"].performed += (InputAction.CallbackContext context) =>
                    {
                        pilotInput.movement = context.ReadValue<Vector2>();
                        if (math.distance(pilotInput.movement, float2.zero) < deadzone)
                            pilotInput.movement = float2.zero;
                    };
                    map["Jump"].performed += (InputAction.CallbackContext context) =>
                    {
                        pilotInput.jumping = (context.ReadValue<float>() > 0.5f);
                    };
                    map["LTrigger"].performed += (InputAction.CallbackContext context) =>
                    {
                        pilotInput.lTrigger = context.ReadValue<float>();
                    };
                    map["RTrigger"].performed += (InputAction.CallbackContext context) =>
                    {
                        pilotInput.rTrigger = context.ReadValue<float>();
                    };
                    break;
            }
        }
    }
    
    public struct PilotInput
    {
        public float2 movement;
        public bool jumping;
        public float lTrigger;
        public float rTrigger;
    }
}
