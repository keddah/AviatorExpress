using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    public Vector2 moveInput { get; private set; }

    public float throttleUpValue { get; private set; }
    public float throttleDownValue { get; private set; }
    
    public bool brakePressed { get; private set; }
    public bool takeOffPressed { get; private set; }
    
    public bool leftPressed { get; private set; }
    public bool rightPressed { get; private set; }
    
    private void Awake()
    {
        throttleUpAction = buttonMapping.FindAction("ThrottleUp");
        throttleDownAction = buttonMapping.FindAction("ThrottleDown");
        
        moveAction = buttonMapping.FindAction("Move");
        
        leftAction = buttonMapping.FindAction("TurnLeft");
        rightAction = buttonMapping.FindAction("TurnRight");
        
        brakeAction = buttonMapping.FindAction("Brake");
        takeOffAction = buttonMapping.FindAction("TakeOff");
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
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
    }
}
