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
    
    public Vector2 moveInput { get; private set; }

    public bool throttleUpPressed { get; private set; }
    public bool throttleDownPressed { get; private set; }
    
    private void Awake()
    {
        throttleUpAction = buttonMapping.FindAction("ThrottleUp");
        throttleDownAction = buttonMapping.FindAction("ThrottleDown");
        moveAction = buttonMapping.FindAction("Move");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        buttonMapping.Enable();
        throttleUpAction.Enable();
        throttleDownAction.Enable();
        moveAction.Enable();
    }

    private void OnDisable()
    {
        buttonMapping.Disable();
        throttleUpAction.Disable();
        throttleDownAction.Disable();
        moveAction.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        throttleUpPressed = throttleUpAction.IsPressed();
        throttleDownPressed = throttleDownAction.IsPressed();
        moveInput = moveAction.ReadValue<Vector2>();
    }
}
