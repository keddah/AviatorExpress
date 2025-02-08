using System;
using UnityEngine;

public class AviatorController : MonoBehaviour
{
    [SerializeField] 
    protected AviatorStats stats;
    
    [Space]
    
    [SerializeField] 
    protected GameObject mainObject;
    
    [SerializeField] 
    protected GameObject mainPropeller;
   
    [SerializeField] 
    protected GameObject centerMass;

    protected Rigidbody mainRb;
    protected Rigidbody mainPropellerRb;

    
    ///////////// Main Propeller
    [SerializeField]
    protected bool invertPitch = true;
    
    [Space]
    
    [Header("Main Propeller")]
    [SerializeField] 
    protected Vector3 mainPropellerSpinAxis = new(0, 1, 0);
    
    // The current spin rate
    protected float mainPropellerSpinRate;
    protected float maxPropellerSpinRate;

    protected float mainPropellerSpeed;

    
    ///////////// Controls
    protected InputController inputManager;


    protected bool engineOn;

    protected float altitude;


    protected virtual void Awake()
    {
        inputManager = GetComponent<InputController>();

        mainRb = mainObject.GetComponent<Rigidbody>();
        mainPropellerRb = mainPropeller.GetComponent<Rigidbody>();

        mainRb.centerOfMass = centerMass.transform.localPosition;

        HingeJoint propellerHinge = mainPropeller.GetComponent<HingeJoint>();
        propellerHinge.axis = mainPropellerSpinAxis;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        mainPropellerRb.maxAngularVelocity = stats.maxMainPropellerAccelSpinRate + 5;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        mainPropellerSpeed = mainPropellerRb.angularVelocity.magnitude;
        altitude = mainRb.transform.position.y;
        
        ThrottleControl();
    }

    protected virtual void FixedUpdate()
    {
        SpinPropeller();
        Lift();
        
        RollControl();
        PitchControl();
        YawControl();
    }

    private void ThrottleControl()
    {
        if (inputManager.throttleUpPressed) maxPropellerSpinRate = stats.maxMainPropellerAccelSpinRate;
        else if (inputManager.throttleDownPressed) maxPropellerSpinRate = stats.maxMainPropellerDecelSpinRate;
        else maxPropellerSpinRate = stats.maxMainPropellerIdleSpinRate;
    }
    
    protected virtual void SpinPropeller()
    {
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            // Main Blade
            mainPropellerSpinRate -= mainPropellerSpinRate * stats.mainPropellerSpinDecel;
            mainPropellerSpinRate = Math.Max(0, mainPropellerSpinRate);

            mainPropellerRb.AddRelativeTorque(mainPropellerSpinAxis * mainPropellerSpinRate);
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        mainPropellerSpinRate = Math.Min(mainPropellerSpinRate * stats.mainPropellerSpinAccel, maxPropellerSpinRate);
        
        // Add torque... Clamp its angular velocity
        mainPropellerRb.AddRelativeTorque(mainPropellerSpinAxis * (mainPropellerSpinRate));
        mainPropellerRb.angularVelocity = Math.Min(mainPropellerRb.angularVelocity.magnitude, maxPropellerSpinRate) * GetPropellerForwardAxis(false);
    }
    
    protected virtual void OnStartEngine()
    {
        engineOn = !engineOn;
        if (engineOn && mainPropellerSpinRate < 1) mainPropellerSpinRate += 1f;
    }
    
    protected virtual void Lift()
    {
        
    }
    
    protected virtual void YawControl()
    {
        
    }
    
    protected virtual void PitchControl()
    {
        
    }
    
    protected virtual void RollControl()
    {
        
    }
    
    protected Vector3 GetPropellerForwardAxis(bool flip = false)
    {
        Vector3 localForward;
        if (mainPropellerSpinAxis.x != 0) localForward = mainPropellerRb.transform.right;
        else if (mainPropellerSpinAxis.y != 0) localForward = mainPropellerRb.transform.up;
        else localForward = mainPropellerRb.transform.forward;

        return flip? -localForward : localForward;
    }
}