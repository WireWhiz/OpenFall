using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public float deadzone;
    public InputActionAsset input;
    public static PilotInput pilotInput;
    
    void Start()
    {
        if(instance && instance != this)
        {
            Destroy(this);
        }
        instance = this;
        foreach(InputActionMap map in input.actionMaps)
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
                        pilotInput.Jump = (context.ReadValue<float>() > 0.5f);
                    };
                    break;
            }
        }

    }
    
    public struct PilotInput
    {
        public float2 movement;
        public bool Jump;
    }
}
