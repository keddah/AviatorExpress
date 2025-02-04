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

    private List<Rigidbody> wingRbs = new();
    
    // left, right
    private List<Rigidbody> aileronRbs = new();
    private List<Rigidbody> flapRbs = new();
    
    private Quaternion[] aileronRotations = new Quaternion [2];
    private Quaternion flapRestingRot;
    private Quaternion elevatorRestingRot;
    
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
    private float rudderToNeutralSpeed = 10;

    [SerializeField] 
    private float aileronPower = 20;
    
    [SerializeField] 
    private float flapPower = 20;
    
    [SerializeField] 
    private float elevatorPower = 300;
    
    [SerializeField] 
    private float aileronToNeutralSpeed = 10;
    
    [SerializeField] 
    private float flapToNeutralSpeed = 10;
    
    [SerializeField] 
    private float elevatorToNeutralSpeed = 10;
    
    [SerializeField]
    private float propellerPowerScaling = 1f;    
    private float altitude;

    [SerializeField, Tooltip("The forward axis for the plane parts (rudder, ailerons, etc...")] 
    private Vector3 chordlineAxis = new(0, 1, 0);

    [SerializeField]
    private bool flipChordlineAxis = true;
    
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
        foreach (GameObject flap in flaps)
        {
            aeroParts.Add(flap.GetComponent<Rigidbody>());
            flapRbs.Add(flap.GetComponent<Rigidbody>());
        }
        flapRestingRot = flaps[0].transform.localRotation;
        
        // Add ailerons to its rigidbody list and aero parts list
        for(int i = 0; i < ailerons.Length; i++)
        {
            aeroParts.Add(ailerons[i].GetComponent<Rigidbody>());
            aileronRbs.Add(ailerons[i].GetComponent<Rigidbody>());
            aileronRotations[i] = ailerons[i].transform.localRotation;
        }
        aeroParts.Add(rudder.GetComponent<Rigidbody>());
        aeroParts.Add(elevator.GetComponent<Rigidbody>());

        elevatorRestingRot = elevator.transform.localRotation;
        
        // Limit how much the elevator is allowed to rotate
        HingeJoint elevatorHinge = elevator.GetComponent<HingeJoint>();
        JointLimits elevatorHingeLimits = new JointLimits { min = -maxElevatorAngle, max = maxElevatorAngle };
        elevatorHinge.useLimits = true;
        elevatorHinge.limits = elevatorHingeLimits;
        
        // Limit how much the rudder is allowed to rotate
        HingeJoint rudderHinge = rudder.GetComponent<HingeJoint>();
        JointLimits rudderHingeLimits = new JointLimits { min = -maxRudderAngle, max = maxRudderAngle };
        rudderHinge.useLimits = true;
        rudderHinge.limits = rudderHingeLimits;
        
        // Limit how much the ailerons are allowed to rotate
        JointLimits aileronLimits = new JointLimits { min = -maxAileronAngle, max = maxAileronAngle };
        foreach (GameObject aileron in ailerons)
        {
            HingeJoint hinge = aileron.GetComponent<HingeJoint>();
            hinge.useLimits = true;
            hinge.limits = aileronLimits;
        }

        // Limit how much the flaps are allowed to rotate
        JointLimits flapLimits = new JointLimits { min = minFlapAngle, max = maxFlapAngle };
        foreach (GameObject flap in flaps)
        {
            HingeJoint hinge = flap.GetComponent<HingeJoint>();
            hinge.useLimits = true;
            hinge.limits = flapLimits;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = .5f;
    }

    // Update is called once per frame
    private void Update()
    {
        altitude = planeRb.transform.position.y;
        
        SpinPropeller();
        
        ThrottleControl();
    }

    private void FixedUpdate()
    {
        Thrust(true);
        YawControl();
        RollControl();
        PitchControl();
        
        // For braking/taking off
        FlapControl();

        foreach (Rigidbody wingSection in wingRbs)
        {
            ApplyAerodynamicForces(wingSection);
        }
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
            // Remove angular velocity
            foreach (Rigidbody flap in flapRbs)
            {
                flap.angularVelocity = Vector3.zero;

                // Manually reset the rotation
                flap.transform.localRotation = 
                    Quaternion.Lerp(flap.transform.localRotation, flapRestingRot, Time.deltaTime * flapToNeutralSpeed);
            }
            return;
        }

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
        if (!inputManager.leftPressed && !inputManager.rightPressed)
        {
            // Remove angular velocity
            rudderRb.angularVelocity = Vector3.zero;
            
            // Manually reset the rotation
            rudderRb.transform.localRotation = 
                Quaternion.Lerp(rudderRb.transform.localRotation, Quaternion.identity, Time.deltaTime * rudderToNeutralSpeed);
            return;
        }

        bool left = inputManager.leftPressed;
        rudderRb.AddRelativeTorque(Vector3.forward * (rudderPower * (left? -1 : 1)));
    }

    private void RollControl()
    {
        float input = inputManager.moveInput.x;
        
        // Reset to neutral position when there's no input
        if (input == 0)
        {
            // Remove angular velocity
            for(int i = 0; i < aileronRbs.Count; i++)
            {
                aileronRbs[i].angularVelocity = Vector3.zero;
                
                // Manually reset the rotation
                aileronRbs[i].transform.localRotation = 
                    Quaternion.Lerp(aileronRbs[i].transform.localRotation, aileronRotations[i], Time.deltaTime * aileronToNeutralSpeed);
            }
            return;
        }
        
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
            elevatorRb.angularVelocity = Vector3.zero;
            elevatorRb.transform.localRotation = 
                    Quaternion.Lerp(elevatorRb.transform.localRotation, elevatorRestingRot, Time.deltaTime * elevatorToNeutralSpeed);
            return;
        }
        
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
        // Get the velocity and calculate airflow direction
        Vector3 velocityAtPoint = planeRb.GetPointVelocity(sectionBody.transform.position);
        float speed = velocityAtPoint.magnitude;
        Vector3 airflow = -velocityAtPoint.normalized;  // Airflow is opposite to velocity

        // Chordline: Use the local forward direction (nose direction of the plane)
        Vector3 chordline = sectionBody.transform.forward;  // Forward direction of the section

        // Debug lines to visualize airflow and chordline
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + chordline * 10, Color.green);
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + airflow * 10, Color.red);

        // Calculate the angle of attack
        float angleOfAttack = Vector3.Angle(chordline, airflow);

        // Lift force: Using sine of angle of attack for more accurate lift calculation
        float liftForce = liftCoefficient * 0.5f * AeroPhysics.GetAirDensity(altitude) * Mathf.Pow(speed, 2) * AeroPhysics.FindWingAreaPerSection(3.4f, 1.8f, 1.5f, 1) * Mathf.Sin(angleOfAttack * Mathf.Deg2Rad);

        // The lift direction is perpendicular to both the chordline and the airflow
        Vector3 liftDirection = Vector3.Cross(airflow, chordline).normalized;
        Quaternion rotation = Quaternion.AngleAxis(-90, sectionBody.transform.up);
        liftDirection = rotation * liftDirection; 
        
        // Debug: Draw the lift direction
        Debug.DrawLine(sectionBody.transform.position, sectionBody.transform.position + liftDirection * 10, Color.cyan);

        // Apply lift at the wing section position
        planeRb.AddForceAtPosition(liftDirection * liftForce, sectionBody.transform.position, ForceMode.Force);

        // Apply drag force (drag is opposite to the airflow direction)
        float dragForce = planeRb.linearDamping * 0.5f * AeroPhysics.GetAirDensity(altitude) * Mathf.Pow(speed, 2) * AeroPhysics.FindWingAreaPerSection(3.4f, 1.8f, 1.5f, 1);
        planeRb.AddForceAtPosition(airflow * dragForce, sectionBody.transform.position, ForceMode.Force);
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