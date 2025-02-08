using System.Collections.Generic;
using UnityEngine;

public class Plane : AviatorController
{
    [Header("Plane Parts")]
    [SerializeField] private GameObject rudder;

    [SerializeField] private GameObject elevator;

    // left, right
    [SerializeField] private GameObject[] wings;
    
    [SerializeField] private GameObject[] flaps;
    [SerializeField] private GameObject[] ailerons;

    private List<GameObject> aeroParts = new();
    
    private Quaternion[] aileronDefaultRots = new Quaternion[2];
    private Quaternion[] flapDefaultRots = new Quaternion[2];
    
    protected override void Awake()
    {
        base.Awake();
        
        // Add all the wing sections list
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
            flap.transform.localRotation = 
                Quaternion.Lerp(flap.transform.localRotation, Quaternion.Euler((up ? stats.maxFlapAngle - 90 : stats.minFlapAngle - 90), 0, 0), Time.deltaTime * stats.flapSpeed);
        }
    }

    protected override void YawControl()
    {
        // Reset to normal position
        if (!inputManager.leftPressed && !inputManager.rightPressed)
        {
            rudder.transform.localRotation = Quaternion.Lerp(rudder.transform.localRotation, Quaternion.identity, Time.deltaTime * stats.rudderToNeutralSpeed);
            return;
        }

        bool left = inputManager.leftPressed;
        rudder.transform.localRotation = 
            Quaternion.Lerp(rudder.transform.localRotation, Quaternion.Euler(0, 0, (left ? stats.maxRudderAngle : -stats.maxRudderAngle)), Time.deltaTime * stats.rudderSpeed);
    }

    protected override void RollControl()
    {
        float input = inputManager.moveInput.x;

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
        
        float targetAngle = stats.maxAileronAngle * input;
        Quaternion leftTargetRotation = aileronDefaultRots[0] * Quaternion.AngleAxis(-targetAngle, Vector3.right);
        Quaternion rightTargetRotation = aileronDefaultRots[1] * Quaternion.AngleAxis(targetAngle, Vector3.right);

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

    protected override void Lift()
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(stats.mainPropellerRadius) * Mathf.Pow(mainPropellerSpeed, 2);
        mainRb.AddForceAtPosition(GetPropellerForwardAxis(true) * (float)(mainThrust * stats.propellerPowerScaling), mainPropellerRb.transform.position);
    }

    void ApplyAerodynamicForces(GameObject sectionBody)
    {
        Vector3 velocity = mainRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
        
        Vector3 airflow = -velocity.normalized;
        Vector3 chordline = sectionBody.transform.forward;
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);

        float angleOfAttack = Vector3.SignedAngle(chordline, airflow, sectionBody.transform.right) * Mathf.Deg2Rad;
    
        float wingArea = AeroPhysics.FindWingAreaPerSection(3.5f, 1.8f, 1.5f, 1);
    
        float liftForce = stats.liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * speed * speed * wingArea * Mathf.Sin(angleOfAttack);
        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.cyan);
        
        mainRb.AddForceAtPosition(liftForce * liftDirection, sectionBody.transform.position);
    }

    void ApplyWingSurfaceForces(GameObject sectionBody)
    {
        Vector3 velocity = mainRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
    
        Vector3 airflow = -velocity.normalized;  
        Vector3 chordline = -sectionBody.transform.up;  

        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.magenta);
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.yellow);

        float angleOfAttack = Vector3.SignedAngle(chordline, airflow, sectionBody.transform.right) * Mathf.Deg2Rad;

        float chordLength = 0;
        float sectionSpan = 0;
        GetSectionDimensions(sectionBody.tag, ref sectionSpan, ref chordLength);
        float wingArea = AeroPhysics.FindWingSectionArea(sectionSpan, chordLength);

        float liftForce = stats.liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * speed * speed * wingArea * Mathf.Sin(angleOfAttack);

        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.blue);
    
        // sectionBody.AddForce(liftDirection * (liftForce * 10));
        mainRb.AddForceAtPosition(liftDirection * (liftForce), sectionBody.transform.position);
    }


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
    
    private Vector3 GetPropellerForwardAxis(bool flip)
    {
        Vector3 localForward;
        if (mainPropellerSpinAxis.x != 0) localForward = mainPropellerRb.transform.right;
        else if (mainPropellerSpinAxis.y != 0) localForward = mainPropellerRb.transform.up;
        else localForward = mainPropellerRb.transform.forward;

        return flip? -localForward : localForward;
    }
}
