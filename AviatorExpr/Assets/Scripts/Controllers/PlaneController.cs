using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Objects")] 
    [SerializeField] private GameObject plane;

    [Space]
    [SerializeField] private GameObject rudder;

    [SerializeField] private GameObject elevator;

    // left, right
    [SerializeField] private GameObject[] wings;
    
    [SerializeField] private GameObject[] flaps;
    [SerializeField] private GameObject[] ailerons;

    [SerializeField] private GameObject propeller;

    [SerializeField] private GameObject centerMass;

    private Rigidbody planeRb;
    private Rigidbody propellerRb;
    private List<GameObject> aeroParts = new();

    
    // left, right
    
    [SerializeField] 
    private float propellerRadius = 1;

    private bool engineOn;

    private float maxPropellerSpinRate;
    
    [Header("Propeller Spin")]
    [SerializeField] 
    private float maxPropellerAccelSpinRate = 300;
    
    [SerializeField] 
    private float maxPropellerIdleSpinRate = 210;
    
    [SerializeField] 
    private float maxPropellerDecelSpinRate = 170;
    
    private float propellerSpinRate;

    [SerializeField] 
    private Vector3 propellerSpinAxis = new(0, 1, 0);
    
    [SerializeField] 
    private float propellerSpinAccel = 1.02f;
    
    [SerializeField] 
    private float propellerSpinDecel = .9f;
    
    private float propellerSpeed;

    private InputController inputManager;
    private bool invertPitch = true;

    [SerializeField] 
    private float maxAileronAngle = 75;
    
    [SerializeField] 
    private float maxFlapAngle = 80;
    [SerializeField] 
    private float minFlapAngle = -60;
    
    [Header("Rudder")]
    [SerializeField] 
    private float maxRudderAngle = 30;

    [SerializeField] 
    private float maxElevatorAngle = 20;
    
    [SerializeField] 
    private float rudderPower = 50;

    [SerializeField] 
    private float aileronPower = 20;
    
    [SerializeField] 
    private float flapPower = 20;
    
    [SerializeField] 
    private float elevatorPower = 300;
    
    [SerializeField] 
    private float rudderToNeutralForce = 10;
    
    [SerializeField] 
    private float aileronToNeutralForce = 10;
    
    [SerializeField] 
    private float flapToNeutralForce = 10;
    
    [SerializeField] 
    private float elevatorToNeutralForce = 10;
    
    [SerializeField]
    private float propellerPowerScaling = .1f;    
    private float altitude;

    [SerializeField, Tooltip("The forward axis for the plane parts (rudder, ailerons, etc...")] 
    private Vector3 chordlineAxis = new(0, 1, 0);

    [SerializeField]
    private bool flipChordlineAxis = true;
    
    private float aileronSpan = .6f;
    private float flapSpan = .6f;
    private float elevatorSpan = 3.8f;
    private float rudderSpan = 1.8f;
    
    private float rudderChord = .4f;
    private float elevatorChord = .7f;
    private float aileronChord = .3f;
    private float flapChord = .3f;

    private Quaternion[] aileronDefaultRots = new Quaternion[2];
    private Quaternion[] flapDefaultRots = new Quaternion[2];
    
    [SerializeField] 
    private float wingspan = 14;
    
    [SerializeField]
    float liftCoefficient = 1.2f;
    
    private void Awake()
    {
        inputManager = GetComponent<InputController>();
        
        planeRb = plane.GetComponent<Rigidbody>();
        
        propellerRb = propeller.GetComponent<Rigidbody>();
        propellerRb.maxAngularVelocity = maxPropellerAccelSpinRate + 5;
        
        planeRb.centerOfMass = centerMass.transform.localPosition;

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
        JointLimits rudderLimits = new JointLimits { max = maxRudderAngle, min = -maxRudderAngle };
        rudderJoint.limits = rudderLimits;
        rudderJoint.useLimits = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        altitude = planeRb.transform.position.y;
        
        ThrottleControl();
    }

    private void FixedUpdate()
    {
        YawControl();
        RollControl();
        PitchControl();
        
        FlapControl();
        
        SpinPropeller();
        Thrust(true);

        // Forces for the wing sections
        foreach (GameObject part in aeroParts) ApplyWingSurfaceForces(part);
        
        // Forces for the wings
        foreach (GameObject wing in wings) ApplyAerodynamicForces(wing);
        
    }

    private void ThrottleControl()
    {
        if (inputManager.throttleDownPressed) maxPropellerSpinRate = maxPropellerDecelSpinRate;
        else if (inputManager.throttleUpPressed) maxPropellerSpinRate = maxPropellerAccelSpinRate;
        else maxPropellerSpinRate = maxPropellerIdleSpinRate;
        propellerRb.maxAngularVelocity = math.lerp(propellerRb.maxAngularVelocity, maxPropellerSpinRate + 5, Time.deltaTime * propellerSpinDecel);
    }

    private void FlapControl()
    {
        // Reset to neutral position when there's no input
        if (!inputManager.brakePressed && !inputManager.takeOffPressed)
        {
            for(var i = 0; i < flaps.Length; i++)
            {
                flaps[i].transform.localRotation = Quaternion.Lerp(flaps[i].transform.localRotation, flapDefaultRots[i], Time.deltaTime * flapToNeutralForce);
            }
            return;
        }
        
        // Either pull the flap up or down
        bool up = inputManager.brakePressed;
        foreach (GameObject flap in flaps)
        {
            // brake makes the flaps tip upwards
            // Otherwise it tips downwards to assist with takeoffs
            flap.transform.localRotation = Quaternion.Lerp(flap.transform.localRotation, Quaternion.Euler((up ? maxFlapAngle - 90 : minFlapAngle - 90), 0, 0), Time.deltaTime * flapPower);
        }
    }
    
    private void YawControl()
    {
        // Reset to normal position
        if (!inputManager.leftPressed && !inputManager.rightPressed)
        {
            rudder.transform.localRotation = Quaternion.Lerp(rudder.transform.localRotation, Quaternion.identity, Time.deltaTime * rudderToNeutralForce);
            return;
        }

        bool left = inputManager.leftPressed;
        rudder.transform.localRotation = Quaternion.Lerp(rudder.transform.localRotation, Quaternion.Euler(0, 0, (left ? maxRudderAngle : -maxRudderAngle)), Time.deltaTime * rudderPower);
    }

    private void RollControl()
    {
        float input = inputManager.moveInput.x;

        if (input == 0)
        {
            for(var i = 0; i < ailerons.Length; i++)
            {
                ailerons[i].transform.localRotation = Quaternion.Lerp(ailerons[i].transform.localRotation, aileronDefaultRots[i], Time.deltaTime * aileronToNeutralForce);
            }
            return;
        }

        GameObject leftAileron = ailerons[0];
        GameObject rightAileron = ailerons[1];
        
        float targetAngle = maxAileronAngle * input;
        Quaternion leftTargetRotation = aileronDefaultRots[0] * Quaternion.AngleAxis(-targetAngle, Vector3.right);
        Quaternion rightTargetRotation = aileronDefaultRots[1] * Quaternion.AngleAxis(targetAngle, Vector3.right);

        leftAileron.transform.localRotation = Quaternion.Lerp(leftAileron.transform.localRotation, leftTargetRotation, Time.deltaTime * aileronPower);
        rightAileron.transform.localRotation = Quaternion.Lerp(rightAileron.transform.localRotation, rightTargetRotation, Time.deltaTime * aileronPower);
    }
    
    private void PitchControl()
    {
        float input = inputManager.moveInput.y;
        
        // Reset to neutral position when there's no input
        if (input == 0)
        {
            elevator.transform.localRotation = Quaternion.Lerp(elevator.transform.localRotation, Quaternion.identity, Time.deltaTime * elevatorToNeutralForce);
            return;
        }

        float targetAngle = maxElevatorAngle * (invertPitch? -input : input);
        elevator.transform.localRotation = Quaternion.Lerp(elevator.transform.localRotation, Quaternion.Euler(targetAngle, 0, 0), Time.deltaTime * elevatorPower);
    }
    
    private void SpinPropeller()
    {
        propellerSpeed = propellerRb.angularVelocity.magnitude;
        
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            propellerSpinRate -= propellerSpinRate * propellerSpinDecel;
            propellerSpinRate = Math.Max(0, propellerSpinRate);
            propellerRb.AddRelativeTorque(-propellerSpinAxis * (propellerSpinRate));
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        propellerSpinRate = Math.Min(propellerSpinRate * propellerSpinAccel, maxPropellerSpinRate);
        
        // Add torque... Clamp its angular velocity
        propellerRb.AddRelativeTorque(propellerSpinAxis * (propellerSpinRate));

        Vector3 angularVelocity = propellerRb.angularVelocity;
        angularVelocity.y = Math.Min(propellerRb.angularVelocity.y, maxPropellerSpinRate);
        propellerRb.angularVelocity = angularVelocity;
    }

    private void Thrust(bool invert)
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(propellerRadius) * Mathf.Pow(propellerSpeed, 2);
        planeRb.AddForceAtPosition((invert ? -GetPropellerForwardAxis() : GetPropellerForwardAxis()) * (float)(mainThrust * propellerPowerScaling), propellerRb.transform.position);
    }
    
    void ApplyAerodynamicForces(GameObject sectionBody)
    {
        Vector3 velocity = planeRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
        
        Vector3 airflow = -velocity.normalized;
        Vector3 chordline = sectionBody.transform.forward;
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);

        float angleOfAttack = Vector3.SignedAngle(chordline, airflow, sectionBody.transform.right) * Mathf.Deg2Rad;
    
        float wingArea = AeroPhysics.FindWingAreaPerSection(3.5f, 1.8f, 1.5f, 1);
    
        float liftForce = liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * speed * speed * wingArea * Mathf.Sin(angleOfAttack);
        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.cyan);
        
        planeRb.AddForceAtPosition(liftForce * liftDirection, sectionBody.transform.position);
    }

    void ApplyWingSurfaceForces(GameObject sectionBody)
    {
        Vector3 velocity = planeRb.GetPointVelocity(sectionBody.transform.position);
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

        float liftForce = liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * speed * speed * wingArea * Mathf.Sin(angleOfAttack);

        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        // Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.blue);
    
        // sectionBody.AddForce(liftDirection * (liftForce * 10));
        planeRb.AddForceAtPosition(liftDirection * (liftForce), sectionBody.transform.position);
    }


    private void GetSectionDimensions(string section, ref float span, ref float chordLength)
    {
        switch (section)
        {
            case "Aileron":
                chordLength = aileronChord;
                span = aileronSpan;
                break;
            
            case "Flap":
                chordLength = flapChord;
                span = flapSpan;
                break;
            
            case "Rudder":
                chordLength = rudderChord;
                span = rudderSpan;
                break;
            
            case "Elevator":
                chordLength = elevatorChord;
                span = elevatorSpan;
                break;
        }
    }


    private Vector3 GetPropellerForwardAxis()
    {
        Vector3 localForward;
        if (propellerSpinAxis == Vector3.right) localForward = propellerRb.transform.right;
        else if (propellerSpinAxis == Vector3.up) localForward = propellerRb.transform.up;
        else localForward = propellerRb.transform.forward;

        return localForward;
    }
    
    // The specific axis for the parts involved in the aero physics 
    private Vector3 GetAeroAxis(Vector3 axisToGet, Transform obj)
    {
        if (axisToGet == Vector3.forward)
        {
            Vector3 localForward;
            
            if (chordlineAxis == Vector3.right) localForward = obj.right;
            else if (chordlineAxis == Vector3.up) localForward = obj.up;
            else localForward = obj.forward;

            return localForward;
        }

        return Vector3.Cross(flipChordlineAxis? -chordlineAxis : chordlineAxis, axisToGet);
    }
    
    public void OnStartEngine()
    {
        engineOn = !engineOn;
        if (engineOn && propellerSpinRate < 1) propellerSpinRate += 1f;

    }

}