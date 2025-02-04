using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaneController : MonoBehaviour
{
    [Header("Objects")] 
    [SerializeField] private GameObject plane;

    [Space]
    [SerializeField] private GameObject rudder;

    [SerializeField] private GameObject elevator;

    // left, right
    [SerializeField] private GameObject[] leftWing;
    [SerializeField] private GameObject[] rightWing;
    
    [SerializeField] private GameObject[] flaps;
    [SerializeField] private GameObject[] ailerons;

    [SerializeField] private GameObject propeller;

    [SerializeField] private GameObject centerMass;

    private Rigidbody planeRb;

    private List<Rigidbody> aeroParts = new();

    private Rigidbody propellerRb;
    private Rigidbody rudderRb;
    private Rigidbody elevatorRb;

    private HingeJoint rudderHinge;
    private HingeJoint elevatorHinge;
    private List<HingeJoint> flapHinges = new();
    private List<HingeJoint> aileronHinges = new();
    
    private List<Rigidbody> wingRbs = new();
    
    // left, right
    private List<Rigidbody> aileronRbs = new();
    private List<Rigidbody> flapRbs = new();
    
    [SerializeField] 
    private float propellerRadius = 1;

    private bool engineOn;

    private float maxPropellerSpinRate;
    
    [Header("Propeller Spin")]
    [SerializeField] 
    private float maxPropellerAccelSpinRate = 300;
    
    [SerializeField] 
    private float maxPropellerIdleSpinRate = 210;
    
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
    private float propellerPowerScaling = 1f;    
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
    
    [SerializeField] 
    private float wingspan = 14;
    
    [SerializeField]
    float liftCoefficient = 1.2f;
    
    private void Awake()
    {
        inputManager = GetComponent<InputController>();
        
        planeRb = plane.GetComponent<Rigidbody>();
        
        rudderRb = rudder.GetComponent<Rigidbody>();
        elevatorRb = elevator.GetComponent<Rigidbody>();
        
        propellerRb = propeller.GetComponent<Rigidbody>();
        propellerRb.maxAngularVelocity = maxPropellerAccelSpinRate + 50;
        
        planeRb.centerOfMass = centerMass.transform.localPosition;

        foreach (GameObject wingSection in leftWing)
        {
            wingRbs.Add(wingSection.GetComponent<Rigidbody>());
        }
        foreach (GameObject wingSection in rightWing)
        {
            wingRbs.Add(wingSection.GetComponent<Rigidbody>());
        }
        
        // Add flaps to its rigidbody list and aero parts list
        JointLimits flapLimits = new JointLimits { min = minFlapAngle, max = maxFlapAngle };
        for(var i = 0; i < flaps.Length; i++)
        {
            aeroParts.Add(flaps[i].GetComponent<Rigidbody>());
            flapRbs.Add(flaps[i].GetComponent<Rigidbody>());
            flapHinges.Add(flaps[i].GetComponent<HingeJoint>());
        
            // Limit how much the flaps are allowed to rotate
            flapHinges[i].useLimits = true;
            flapHinges[i].limits = flapLimits;
            flapHinges[i].spring = new JointSpring { spring = flapToNeutralForce * 1000, damper = 250 };
        }
        
        // Add ailerons to its rigidbody list and aero parts list
        JointLimits aileronLimits = new JointLimits { min = -maxAileronAngle, max = maxAileronAngle };
        for(var i = 0; i < ailerons.Length; i++)
        {
            aeroParts.Add(ailerons[i].GetComponent<Rigidbody>());
            aileronRbs.Add(ailerons[i].GetComponent<Rigidbody>());
            aileronHinges.Add(ailerons[i].GetComponent<HingeJoint>());
            
            // Limit how much the ailerons are allowed to rotate
            aileronHinges[i].useLimits = true;
            aileronHinges[i].limits = aileronLimits;
            aileronHinges[i].spring = new JointSpring { spring = aileronToNeutralForce * 1000, damper = 250 };
        }
        aeroParts.Add(rudder.GetComponent<Rigidbody>());
        aeroParts.Add(elevator.GetComponent<Rigidbody>());

        elevatorHinge = elevator.GetComponent<HingeJoint>();
        rudderHinge = rudder.GetComponent<HingeJoint>();
        
        // Limit how much the elevator is allowed to rotate
        JointLimits elevatorHingeLimits = new JointLimits { min = -maxElevatorAngle, max = maxElevatorAngle };
        elevatorHinge.useLimits = true;
        elevatorHinge.limits = elevatorHingeLimits;
        elevatorHinge.spring = new JointSpring { spring = elevatorToNeutralForce * 1000, damper = 250 };
        
        // Limit how much the rudder is allowed to rotate
        JointLimits rudderHingeLimits = new JointLimits { min = -maxRudderAngle, max = maxRudderAngle };
        rudderHinge.useLimits = true;
        rudderHinge.limits = rudderHingeLimits;
        rudderHinge.spring = new JointSpring { spring = rudderToNeutralForce * 1000, damper = 250 };
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        altitude = planeRb.transform.position.y;
        
        SpinPropeller();
        ThrottleControl();
        YawControl();
        RollControl();
        PitchControl();
        
        // For braking/taking off
        FlapControl();
    }

    private void FixedUpdate()
    {
        Thrust(true);

        // Forces for the wings
        foreach (Rigidbody wingSection in wingRbs) ApplyAerodynamicForces(wingSection);
        
        // Forces for the wing sections
        foreach (Rigidbody part in aeroParts) ApplyWingSurfaceForces(part);
    }

    private void ThrottleControl()
    {
        maxPropellerSpinRate = inputManager.throttleUpPressed ? maxPropellerAccelSpinRate : maxPropellerIdleSpinRate;
    }

    private void FlapControl()
    {
        // Reset to neutral position when there's no input
        if (!inputManager.throttleDownPressed && !inputManager.brakePressed && !inputManager.takeOffPressed)
        {
            foreach (HingeJoint hinge in flapHinges) hinge.useSpring = true;
            return;
        }
        
        // Turn off spring
        foreach (HingeJoint hinge in flapHinges) hinge.useSpring = false;
        
        bool brake = inputManager.throttleDownPressed || inputManager.brakePressed;
        foreach (Rigidbody flap in flapRbs)
        {
            // brake makes the flaps tip upwards
            // Otherwise it tips downwards to assist with takeoffs
            flap.AddRelativeTorque(Vector3.right * (brake ? flapPower : -flapPower));
        }
    }
    
    private void YawControl()
    {
        // Reset to normal position
        if (!inputManager.leftPressed && !inputManager.rightPressed)
        {
            rudderHinge.useSpring = true;
            return;
        }

        // Turn off spring
        rudderHinge.useSpring = false;
        
        bool left = inputManager.leftPressed;
        rudderRb.AddRelativeTorque(Vector3.forward * (rudderPower * (left? -1 : 1)));
    }

    private void RollControl()
    {
        float input = inputManager.moveInput.x;
        
        // Reset to neutral position when there's no input
        if (input == 0)
        {
            foreach (HingeJoint joint in aileronHinges) joint.useSpring = true;
            return;
        }
        
        foreach (HingeJoint joint in aileronHinges) joint.useSpring = false;
        
        // Assign left and right ailerons
        Rigidbody leftAileron = aileronRbs[0];
        Rigidbody rightAileron = aileronRbs[1];

        // Add opposing torque
        // leftAileron.AddRelativeTorque(GetAeroAxis(Vector3.forward, leftAileron.transform) * (aileronPower * input.x));
        // rightAileron.AddRelativeTorque(GetAeroAxis(Vector3.forward, leftAileron.transform) * (aileronPower * -input.x));
        
        leftAileron.AddRelativeTorque(Vector3.right * (aileronPower * input));
        rightAileron.AddRelativeTorque(Vector3.right * (aileronPower * -input));
    }
    
    private void PitchControl()
    {
        float input = inputManager.moveInput.y;
        
        // Reset to neutral position when there's no input
        if (input == 0)
        {
            elevatorHinge.useSpring = true;
            return;
        }
        
        elevatorHinge.useSpring = false;
        
        elevatorRb.AddRelativeTorque(Vector3.right * (elevatorPower * (invertPitch ? -input : input)));
    }
    
    private void SpinPropeller()
    {
        propellerSpeed = propellerRb.angularVelocity.magnitude;
        
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            propellerSpinRate -= propellerSpinRate * propellerSpinDecel;
            propellerSpinRate = Math.Max(0, propellerSpinRate);
            propellerRb.AddTorque(propellerRb.transform.up * (propellerSpinRate));
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        propellerSpinRate = Math.Min(propellerSpinRate * propellerSpinAccel, maxPropellerSpinRate);
        
        // Add torque... Clamp its angular velocity
        propellerRb.AddTorque(propellerRb.transform.up * (propellerSpinRate));

        propellerRb.angularVelocity = Math.Min(propellerRb.angularVelocity.magnitude, maxPropellerSpinRate) * GetPropellerForwardAxis();
    }

    private void Thrust(bool invert)
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(propellerRadius) * Mathf.Pow(propellerSpeed, 2);
        planeRb.AddForceAtPosition((invert ? -GetPropellerForwardAxis() : GetPropellerForwardAxis()) * (float)(mainThrust * propellerPowerScaling), propellerRb.transform.position);
    }
    
    void ApplyAerodynamicForces(Rigidbody sectionBody)
    {
        Vector3 velocity = planeRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
        
        Vector3 airflow = -velocity.normalized;
        Vector3 chordline = sectionBody.transform.forward;
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);

        float angleOfAttack = Vector3.Dot(chordline, airflow);
        
        float wingArea = AeroPhysics.FindWingAreaPerSection(3.5f, 1.8f, 1.5f, 1);
        
        float liftForce = liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * (float) Math.Pow(speed, 2) * wingArea * Mathf.Clamp(angleOfAttack, -1f, 1f);
        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.cyan);
        
        sectionBody.AddForce(liftForce * liftDirection);
    }

    void ApplyWingSurfaceForces(Rigidbody sectionBody)
    {
        Vector3 velocity = sectionBody.GetPointVelocity(sectionBody.transform.position);
        float speed = velocity.magnitude;
        
        Vector3 airflow = -velocity.normalized;
        print(airflow);
        Vector3 chordline = -sectionBody.transform.up;
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);

        float angleOfAttack = Vector3.Dot(chordline, airflow);

        float chordLength = 0;
        float sectionSpan = 0;
        
        GetSectionDimensions(sectionBody.tag, ref sectionSpan, ref chordLength);

        float wingArea = AeroPhysics.FindWingSectionArea(sectionSpan, chordLength);

        float liftForce = liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * (float) Math.Pow(speed, 2) * wingArea * Mathf.Clamp(angleOfAttack, -1f, 1f);
        Vector3 liftDirection = Vector3.Cross(airflow, sectionBody.transform.right).normalized;
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.blue);
        
        sectionBody.AddForce(liftDirection * ((liftForce) * 10000000000));
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