/**************************************************************************************************************
* Plane Controller ~ Inherits from AviatorController
* 
*   Plane aviators generate lift from the wings and their moving parts. They're granted control using the moving parts. By changing the direction/chordline
*   of the wing part, the plane can rotate.  
*
*   Overrides:
*       Awake()
*       Update()
*       FixedUpdate()
*       PitchControl()
*       YawControl()
*       RollControl()
*       Lift()
* 
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Plane : AviatorController
{
    [Space] 
    [SerializeField] 
    private Vector3 wingSectionChordlineAxis = new (0,0,1);
    
    [SerializeField] 
    private Vector3 wingChordlineAxis = new (0,1,0);
    
    [Space]
    
    [Header("Plane Parts")]
    [SerializeField] private GameObject rudder;
    [SerializeField] private GameObject elevator;

    // left, right
    [SerializeField] private GameObject[] wings;
    [SerializeField] private GameObject[] flaps;
    [SerializeField] private GameObject[] ailerons;

    private List<GameObject> aeroParts = new();

    private Quaternion rudderDefaultRot;
    private Quaternion[] aileronDefaultRots = new Quaternion[2];
    private Quaternion[] flapDefaultRots = new Quaternion[2];

    [Header("Wing VFX")]
    [SerializeField]
    private VisualEffect[] wingFx;

    [SerializeField]
    private ushort fxActivationSpeed = 40;
    
    [SerializeField]
    private float fxActivationTorque = .4f;

    private bool[] wingFxPlaying = {false, false};
    
    protected override void Awake()
    {
        base.Awake();

        rudderDefaultRot = rudder.transform.localRotation;
        
        // Add all the wing sections to the list
        for (var i = 0; i < flaps.Length; i++)
        {
            aeroParts.Add(flaps[i]);
            flapDefaultRots[i] = flaps[i].transform.localRotation;
        }
        for (var i = 0; i < ailerons.Length; i++)
        {
            aeroParts.Add(ailerons[i]);
            aileronDefaultRots[i] = ailerons[i].transform.localRotation;
        }
        aeroParts.Add(rudder);
        aeroParts.Add(elevator);

        
        // Clamp rudder angle
        HingeJoint rudderJoint = rudder.GetComponent<HingeJoint>();
        JointLimits rudderLimits = new JointLimits { max = stats.maxRudderAngle, min = -stats.maxRudderAngle };
        rudderJoint.limits = rudderLimits;
        rudderJoint.useLimits = true;
    }

    protected override void Update()
    {
        base.Update();
        
        WingTrailActivation();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        FlapControl();
        
        // Forces for the wing sections
        foreach (GameObject part in aeroParts) ApplyWingSurfaceForces(part);
        
        // Forces for the wings
        foreach (GameObject wing in wings) ApplyAerodynamicForces(wing);
    }

    private void FlapControl()
    {
        // Reset to neutral position when there's no input
        if (!inputManager.brakePressed && !inputManager.takeOffPressed)
        {
            for(var i = 0; i < flaps.Length; i++)
            {
                flaps[i].transform.localRotation = Quaternion.Lerp(flaps[i].transform.localRotation, flapDefaultRots[i], Time.deltaTime * stats.flapToNeutralSpeed);
            }
            return;
        }
        
        // Either pull the flap up or down
        bool up = inputManager.brakePressed;
        foreach (GameObject flap in flaps)
        {
            // brake makes the flaps tip upwards
            // Otherwise it tips downwards to assist with takeoffs
            flap.transform.localRotation =                                                          // + 90 since the flap's default rotation is 90
                Quaternion.Lerp(flap.transform.localRotation, Quaternion.Euler(up ? stats.minFlapAngle + 90 : stats.maxFlapAngle + 90, 0, 0), Time.deltaTime * stats.flapSpeed);
        }
    }

    protected override void YawControl()
    {
        // Reset to normal position
        if (!inputManager.leftPressed && !inputManager.rightPressed)
        {
            rudder.transform.localRotation = Quaternion.Lerp(rudder.transform.localRotation, rudderDefaultRot, Time.deltaTime * stats.rudderToNeutralSpeed);
            return;
        }

        bool left = inputManager.leftPressed;
        rudder.transform.localRotation = 
            Quaternion.Lerp(rudder.transform.localRotation, Quaternion.Euler(0, (left ? -stats.maxRudderAngle : stats.maxRudderAngle), 0), Time.deltaTime * stats.rudderSpeed);
    }

    protected override void RollControl()
    {
        float input = inputManager.moveInput.x;

        // Move them back to their default rotations
        if (input == 0)
        {
            for(var i = 0; i < ailerons.Length; i++)
            {
                ailerons[i].transform.localRotation = Quaternion.Lerp(ailerons[i].transform.localRotation, aileronDefaultRots[i], Time.deltaTime * stats.aileronToNeutralSpeed);
            }
            return;
        }

        GameObject leftAileron = ailerons[0];
        GameObject rightAileron = ailerons[1];
        
        // Rotate the ailerons in opposite directions
        float targetAngle = stats.maxAileronAngle * input;
        Quaternion leftTargetRotation = aileronDefaultRots[0] * Quaternion.AngleAxis(targetAngle, Vector3.right);
        Quaternion rightTargetRotation = aileronDefaultRots[1] * Quaternion.AngleAxis(-targetAngle, Vector3.right);

        leftAileron.transform.localRotation = Quaternion.Lerp(leftAileron.transform.localRotation, leftTargetRotation, Time.deltaTime * stats.aileronSpeed);
        rightAileron.transform.localRotation = Quaternion.Lerp(rightAileron.transform.localRotation, rightTargetRotation, Time.deltaTime * stats.aileronSpeed);
    }

    protected override void PitchControl()
    {
        float input = inputManager.moveInput.y;
        
        // Reset to neutral position when there's no input
        if (input == 0)
        {
            elevator.transform.localRotation = Quaternion.Lerp(elevator.transform.localRotation, Quaternion.identity, Time.deltaTime * stats.elevatorToNeutralSpeed);
            return;
        }

        float targetAngle = stats.maxElevatorAngle * (invertPitch? -input : input);
        elevator.transform.localRotation = Quaternion.Lerp(elevator.transform.localRotation, Quaternion.Euler(targetAngle, 0, 0), Time.deltaTime * stats.elevatorSpeed);
    }

    // For the propellers...
    protected override void Lift()
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(stats.mainPropellerRadius) * Mathf.Pow(mainPropellerSpeed, 2);
        mainRb.AddForceAtPosition(GetPropellerForwardAxis(true) * (float)(mainThrust * stats.propellerPowerScaling), mainPropellerRb.transform.position);
    }

    // Lift generated from the wings...
    void ApplyAerodynamicForces(GameObject sectionBody)
    {
        Vector3 velocity = mainRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
        
        // Opposite the direction of velocity
        Vector3 airflow = -velocity.normalized;
        
        // The forward axis of the wing section
        Vector3 chordline = GetLocalAxis(wingChordlineAxis, sectionBody.transform);
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);

        float angleOfAttack = Vector3.SignedAngle(chordline, airflow, sectionBody.transform.right) * Mathf.Deg2Rad;
        float wingArea = AeroPhysics.FindWingAreaPerSection(3.5f, 1.8f, 1.5f, 1);
    
        float liftForce = stats.liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * speed * speed * wingArea * Mathf.Sin(angleOfAttack);
        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.blue);
        
        mainRb.AddForceAtPosition(liftForce * liftDirection, sectionBody.transform.position);
    }

    // Lift generated from the wing sections...
    void ApplyWingSurfaceForces(GameObject sectionBody)
    {
        Vector3 velocity = mainRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
    
        // Opposite the direction of velocity
        Vector3 airflow = -velocity.normalized;  
        
        // The forward axis of the wing section
        Vector3 chordline = GetLocalAxis(wingSectionChordlineAxis, sectionBody.transform, true);  
        float angleOfAttack = Vector3.SignedAngle(chordline, airflow, sectionBody.transform.right) * Mathf.Deg2Rad;
        
        // Making the aileron's less dependent on the angle of attack to make rolling more responsive
        if (sectionBody.CompareTag("Aileron")) angleOfAttack *= .5f;

        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);

        float chordLength = 0, sectionSpan = 0;
        GetSectionDimensions(sectionBody.tag, ref sectionSpan, ref chordLength);
        float wingArea = sectionSpan * chordLength;

        float liftForce = stats.liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * speed * speed * wingArea * Mathf.Sin(angleOfAttack);
        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        
        // Allowing the rudder to generate lift horizontally
        if (sectionBody.CompareTag("Rudder")) liftDirection = chordline;

        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.blue);
    
        mainRb.AddForceAtPosition(liftDirection * liftForce, sectionBody.transform.position);
    }


    ////////////////////////////////// ASSIGN THE PLANE SECTION TAGS IN EDITOR... //////////////////////////////////
    private void GetSectionDimensions(string section, ref float span, ref float chordLength)
    {
        switch (section)
        {
            case "Aileron":
                chordLength = stats.aileronChord;
                span = stats.aileronSpan;
                break;
            
            case "Flap":
                chordLength = stats.flapChord;
                span = stats.flapSpan;
                break;
            
            case "Rudder":
                chordLength = stats.rudderChord;
                span = stats.rudderSpan;
                break;
            
            case "Elevator":
                chordLength = stats.elevatorChord;
                span = stats.elevatorSpan;
                break;
        }
    }

    // Gets the forward axis depending on the target axis
    private static Vector3 GetLocalAxis(Vector3 targetAxis, Transform obj, bool flip = false)
    {
        Vector3 localForward;
        if (targetAxis.x != 0) localForward = obj.right;
        else if (targetAxis.y != 0) localForward = obj.up;
        else localForward = obj.forward;

        return flip? -localForward : localForward;
    }

    // For the VFX that play at the end of each wing
    private void WingTrailActivation()
    {
        float speed = mainRb.linearVelocity.magnitude;
        float torque = mainRb.angularVelocity.magnitude;
        bool on = speed >= fxActivationSpeed && torque >= fxActivationTorque;

        // Without separating the playing bools, if one is supposed to be on/off they'll both be on/off
        for(var i = 0; i < wingFx.Length; i++)
        {
            if (on && !wingFxPlaying[i]) 
            {
                wingFx[i].Play();
                wingFxPlaying[i] = true;
            }
            else if (!on) 
            {
                wingFx[i].Stop();
                wingFxPlaying[i] = false;
            }
        }
    }
}
