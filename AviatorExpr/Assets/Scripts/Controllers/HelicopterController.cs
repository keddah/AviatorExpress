/**************************************************************************************************************
* Helicopter Controller ~ Inherits from AviatorController
* 
*   Helicopter aviators generate lift depending on the speed of the propeller. The tail propeller is used for generating horizontal lift (changing yaw).  
*   When pitching/rolling the helicopter, forces are applied rather than having it be completely procedural.
*
*   Overrides:
*       Awake()
*       Start()
*       Update()
*       FixedUpdate()
*       OnEngineStart() 
*       SpinPropeller() 
*       YawControl()
*       Move()
*       Lift()
* 
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System;
using UnityEngine;

public class Helicopter : AviatorController
{
    [Header("Tail Propeller")]
    [SerializeField] 
    private Vector3 tailPropellerSpinAxis;
    
    [SerializeField] 
    private GameObject tailBlade;
    private Rigidbody tailBladeRb;
    
    private float tailBladeSpeed = 0;
    private float maxTailSpinRate;
    private float currentTailSpinRate = 0;

    private bool turningLeft;

    
    protected override void Awake()
    {
        base.Awake();
        
        tailBladeRb = tailBlade.GetComponent<Rigidbody>();
        
        HingeJoint propellerHinge = tailBlade.GetComponent<HingeJoint>();
        propellerHinge.axis = tailPropellerSpinAxis;
    }

    protected override void Start()
    {
        base.Start();
        tailBladeRb.maxAngularVelocity = stats.maxTailPropellerAccelSpinRate + 5;
    }

    protected override void Update()
    {
        // ThrottleControl() being run and altitude + main propeller spin speed being set
        base.Update();
        altitude *= .75f;
        tailBladeSpeed = tailBladeRb.angularVelocity.magnitude;

        YawControl();
    }

    protected override void FixedUpdate()
    {
        SpinPropeller();
        Lift();
        GyroControl();
    }

    protected override void OnStartEngine()
    {
        base.OnStartEngine();
        if (engineOn && currentTailSpinRate < 1) currentTailSpinRate += 1f;
    }

    protected override void Lift()
    {
        // Lift Equation: L = 0.5 * airDensity * velocity² * area * lift coefficient
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(stats.mainPropellerRadius) * Mathf.Pow(mainPropellerSpeed, 2);
        mainRb.AddForceAtPosition(mainPropellerRb.transform.up * (float)(mainThrust * stats.propellerPowerScaling), mainPropellerRb.transform.position);

        // Tail Lift (Yaw control)
        double tailThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * Mathf.Pow(tailBladeSpeed, 2) * AeroPhysics.GetBladeArea(stats.tailPropellerRadius) * (turningLeft ? 1 : -1);
        mainRb.AddForceAtPosition(-tailBladeRb.transform.forward * (float)(tailThrust * stats.propellerPowerScaling), tailBladeRb.transform.position);
    }

    // The exact same function but including the tail propeller
    protected override void SpinPropeller()
    {
        base.SpinPropeller();
        
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            // Tail blade
            currentTailSpinRate -= currentTailSpinRate * stats.tailPropellerSpinDecel;
            currentTailSpinRate = Math.Max(0, currentTailSpinRate);
            
            tailBladeRb.AddRelativeTorque(tailPropellerSpinAxis * currentTailSpinRate); 
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        currentTailSpinRate = Math.Min(currentTailSpinRate * stats.tailPropellerSpinAccel, maxTailSpinRate);
        
        // Add torque... Clamp its angular velocity
        tailBladeRb.AddRelativeTorque(tailPropellerSpinAxis * currentTailSpinRate); 
        tailBladeRb.angularVelocity = Math.Clamp(tailBladeRb.angularVelocity.magnitude, -maxTailSpinRate, maxTailSpinRate) * tailBladeRb.transform.forward;
    }

    // Works similarly to how throttle control works
    protected override void YawControl()
    {
        // Flip the direction in SpinBlades()
        turningLeft = inputManager.leftPressed;
        
        if (inputManager.rightPressed || turningLeft) maxTailSpinRate = stats.maxTailPropellerAccelSpinRate;
        else maxTailSpinRate = stats.maxTailPropellerIdleSpinRate;
    }

    // Tries to keep the helicopter perfectly upright
    private void Stabiliser()
    {
        Vector3 correctionVector = Vector3.Cross(GetUpAxis(),Vector3.up);
        mainRb.AddTorque(correctionVector * stats.stabilisationStrength);
    }
    
    // Eases the helicopters angular velocity to make it more controllable
    private void GyroAssist()
    {
        // Constantly fight against the angular velocity to try to stabilise
        mainRb.angularVelocity = Vector3.Lerp(mainRb.angularVelocity, Vector3.zero, Time.deltaTime * stats.gyroAssistStrength);
        
        Stabiliser();
    }
    
    // Roll / Pitch control
    private void GyroControl()
    {
        Vector2 input = inputManager.moveInput;
        Vector3 velocityToAdd = new();
        
        // Roll
        velocityToAdd.z = -input.x * stats.gyroPower * stats.rollDamping;
        
        // Pitch
        velocityToAdd.x = input.y * stats.gyroPower * (invertPitch? -1 : 1);
        mainRb.AddRelativeTorque(velocityToAdd);
        
        GyroAssist();
    }

    // Addition - The tail propeller start spinning
    public override void Move(Vector3 pos, Quaternion rot)
    {
        base.Move(pos, rot);

        currentTailSpinRate = stats.maxTailPropellerIdleSpinRate;
        maxTailSpinRate = stats.maxTailPropellerIdleSpinRate;
    }
}
