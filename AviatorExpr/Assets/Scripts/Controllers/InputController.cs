/**************************************************************************************************************
* Input Controller  
*
*   Allows inputs to be read all in one place.  
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputController : MonoBehaviour
{
    [SerializeField] 
    private InputActionAsset buttonMapping;
    
    private InputAction throttleUpAction;
    private InputAction throttleDownAction;
    private InputAction moveAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction brakeAction;
    private InputAction takeOffAction;
    private InputAction respawnAction;
    
    public Vector2 moveInput { get; private set; }

    public float throttleUpValue { get; private set; }
    public float throttleDownValue { get; private set; }
    
    public bool brakePressed { get; private set; }
    public bool takeOffPressed { get; private set; }
    
    public bool leftPressed { get; private set; }
    public bool rightPressed { get; private set; }
    
    public bool respawnPressed { get; private set; }
    
    private float respawnStartTime;
    
    public float currenetRespawnHoldTime { get; private set; }
    public float respawnActionHoldTime { get; private set; }

    
    private void Awake()
    {
        throttleUpAction = buttonMapping.FindAction("ThrottleUp");
        throttleDownAction = buttonMapping.FindAction("ThrottleDown");
        
        moveAction = buttonMapping.FindAction("Move");
        
        leftAction = buttonMapping.FindAction("TurnLeft");
        rightAction = buttonMapping.FindAction("TurnRight");
        
        brakeAction = buttonMapping.FindAction("Brake");
        takeOffAction = buttonMapping.FindAction("TakeOff");
        
        respawnAction = buttonMapping.FindAction("Respawn");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        buttonMapping.Enable();
        
        throttleUpAction.Enable();
        throttleDownAction.Enable();
        
        moveAction.Enable();
        
        leftAction.Enable();
        rightAction.Enable();
        
        brakeAction.Enable();
        takeOffAction.Enable();
        
        respawnAction.Enable();
        
        // Get the hold time
        respawnAction.started += ctx =>
        {
            if (ctx.interaction is not HoldInteraction hold) return;
            respawnActionHoldTime = hold.duration;
        };
    }

    private void OnDisable()
    {
        buttonMapping.Disable();
        
        throttleUpAction.Disable();
        throttleDownAction.Disable();
        
        moveAction.Disable();
        
        leftAction.Disable();
        rightAction.Disable();

        brakeAction.Disable();
        takeOffAction.Disable();
        
        respawnAction.Disable();
    }

    private void Start()
    {
        LockMouse(0);
    }

    // Update is called once per frame
    void Update()
    {
        throttleUpValue = throttleUpAction.ReadValue<float>();
        throttleDownValue = throttleDownAction.ReadValue<float>();
        
        moveInput = moveAction.ReadValue<Vector2>();
        
        leftPressed = leftAction.IsPressed();
        rightPressed = rightAction.IsPressed();

        brakePressed = brakeAction.IsPressed();
        takeOffPressed = takeOffAction.IsPressed();

        CalculateRespawnTime();
    }
    
    public void LockMouse(ushort num) { Cursor.lockState = CursorLockMode.Locked; }
    public void UnlockMouse() { Cursor.lockState = CursorLockMode.None; }

    void CalculateRespawnTime()
    {
        // If button is first pressed, store the start time
        if (!respawnPressed && respawnAction.inProgress) respawnStartTime = Time.time;
        respawnPressed = respawnAction.inProgress;

        // Reset if button is released
        if (!respawnPressed)
        {
            respawnStartTime = -1;
            currenetRespawnHoldTime = 0; // Reset hold time
            return;
        }

        // Calculate current hold time
        currenetRespawnHoldTime = Time.time - respawnStartTime;
    }

}
