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
    [SerializeField] private GameObject[] flaps;
    [SerializeField] private GameObject[] ailerons;

    [SerializeField] private GameObject propeller;

    [SerializeField] private GameObject centerMass;

    private Rigidbody planeRb;
    [SerializeField] private GameObject[] wingTransforms;

    private List<Rigidbody> aeroParts = new();

    private Rigidbody propellerRb;
    private Rigidbody rudderRb;
    private Rigidbody elevatorRb;
    
    // left, right
    private List<Rigidbody> aileronRbs = new();
    private List<Rigidbody> flapRbs = new();
    private Quaternion[] aileronRotations = new Quaternion [2];
    
    [SerializeField] 
    private float propellerRadius = 1;

    private bool engineOn;

    private float maxPropellerSpinRate;
    
    private float aileronSpan = .6f;
    private float flapSpan = .6f;
    private float elevatorSpan = 3.8f;
    private float rudderSpan = 1.8f;
    
    private float rudderChord = .4f;
    private float elevatorChord = .7f;
    private float aileronChord = .3f;
    private float flapChord = .3f;
    
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
    private float wingspan = 14;
    
    [SerializeField]
    float liftCoefficient = .5f;
    
    private void Awake()
    {
        inputManager = GetComponent<InputController>();
        
        planeRb = plane.GetComponent<Rigidbody>();
        
        rudderRb = rudder.GetComponent<Rigidbody>();
        elevatorRb = elevator.GetComponent<Rigidbody>();
        
        propellerRb = propeller.GetComponent<Rigidbody>();
        propellerRb.maxAngularVelocity = maxPropellerAccelSpinRate + 50;
        
        planeRb.centerOfMass = centerMass.transform.localPosition;
        
        // Add flaps to its rigidbody list and aero parts list
        foreach (GameObject flap in flaps)
        {
            aeroParts.Add(flap.GetComponent<Rigidbody>());
            flapRbs.Add(flap.GetComponent<Rigidbody>());
        }
        
        // Add ailerons to its rigidbody list and aero parts list
        for(int i = 0; i < ailerons.Length; i++)
        {
            aeroParts.Add(ailerons[i].GetComponent<Rigidbody>());
            aileronRbs.Add(ailerons[i].GetComponent<Rigidbody>());
            aileronRotations[i] = ailerons[i].transform.localRotation;
        }
        aeroParts.Add(rudder.GetComponent<Rigidbody>());
        aeroParts.Add(elevator.GetComponent<Rigidbody>());
        
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

        foreach (GameObject wing in wingTransforms)
        {
            ApplyAerodynamicForces(wing.transform);
        }
        
        foreach (Rigidbody wingSection in aeroParts)
        {
            ApplyAerodynamicSectionForces(wingSection.transform);
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
                    Quaternion.Lerp(flap.transform.localRotation, Quaternion.identity, Time.deltaTime * flapToNeutralSpeed);
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
                    Quaternion.Lerp(elevatorRb.transform.localRotation, Quaternion.identity, Time.deltaTime * elevatorToNeutralSpeed);
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
    
    void ApplyAerodynamicForces(Transform wing)
    {
        Vector3 velocityAtPoint = planeRb.GetPointVelocity(wing.position);
        float speed = velocityAtPoint.magnitude;
        Vector3 airflow = -velocityAtPoint.normalized;  // Airflow is opposite to the velocity
    
        // Get the chordline direction (ensure it's the right one)
        Vector3 chordline = wing.forward;

        // Debug: Visualize the airflow and chordline vectors
        Debug.DrawRay(wing.position, airflow * 10f, Color.red, 0.1f); // Airflow vector (opposite of velocity)
        Debug.DrawRay(wing.position, chordline * 10f, Color.green, 0.1f); // Chordline vector

        // Calculate the angle of attack
        float angleOfAttack = Vector3.Angle(chordline, airflow);
        print("Angle of Attack: " + angleOfAttack);  // Check if the angle changes

        float airDensity = AeroPhysics.GetAirDensity(altitude);
        float wingAreaPerSection = AeroPhysics.FindWingAreaPerSection();

        // Calculate lift force
        float liftForce = liftCoefficient * 0.5f * airDensity * speed * speed * wingAreaPerSection * Mathf.Sin(angleOfAttack * Mathf.Deg2Rad);
        Vector3 liftDirection = Vector3.Cross(airflow, chordline).normalized;
    
        // Apply lift at the wing section position
        planeRb.AddForceAtPosition(liftDirection * liftForce, wing.position);

        // Calculate and apply drag force
        float stallFactor = Mathf.Clamp01(Mathf.Abs(angleOfAttack) / 15f);
        float dragForce = planeRb.linearDamping * 0.5f * airDensity * speed * speed * wingAreaPerSection * (1f + stallFactor * 2f);
        Vector3 dragDirection = -airflow;
        planeRb.AddForceAtPosition(dragDirection * dragForce, wing.position);
    }
    
    void ApplyAerodynamicSectionForces(Transform section)
    {
        Vector3 velocityAtPoint = planeRb.GetPointVelocity(section.position);
        float speed = velocityAtPoint.magnitude;
        Vector3 airflow = -velocityAtPoint.normalized;  // Airflow is opposite to the velocity
    
        // Get the chordline direction (ensure it's the right one)
        Vector3 chordline = -section.up;

        // Debug: Visualize the airflow and chordline vectors
        Debug.DrawRay(section.position, airflow * 10f, Color.red, 0.1f); // Airflow vector (opposite of velocity)
        Debug.DrawRay(section.position, chordline * 10f, Color.green, 0.1f); // Chordline vector

        // Calculate the angle of attack
        float angleOfAttack = Vector3.Angle(chordline, airflow);
        print("Angle of Attack: " + angleOfAttack);  // Check if the angle changes

        float airDensity = AeroPhysics.GetAirDensity(altitude);
        float wingAreaPerSection = GetSectionArea(section.tag);

        // Calculate lift force
        float liftForce = liftCoefficient * 0.5f * airDensity * speed * speed * wingAreaPerSection * Mathf.Sin(angleOfAttack * Mathf.Deg2Rad);
        Vector3 liftDirection = Vector3.Cross(airflow, chordline).normalized;
    
        // Apply lift at the wing section position
        planeRb.AddForceAtPosition(liftDirection * liftForce, section.position);

        // Calculate and apply drag force
        float stallFactor = Mathf.Clamp01(Mathf.Abs(angleOfAttack) / 15f);
        float dragForce = planeRb.linearDamping * 0.5f * airDensity * speed * speed * wingAreaPerSection * (1f + stallFactor * 2f);
        Vector3 dragDirection = -airflow;
        planeRb.AddForceAtPosition(dragDirection * dragForce, section.position);
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

            // Check if the chordlineAxis is reversed or aligned differently
            if (chordlineAxis == Vector3.right) 
                localForward = obj.right;
            else if (chordlineAxis == Vector3.up) 
                localForward = obj.up;
            else 
                localForward = obj.forward;

            return localForward;
        }

        return Vector3.Cross(chordlineAxis, axisToGet);
    }

    private float GetSectionArea(string tag)
    {
        float area = 0;
        switch (tag)
        {
            case "aileron":
                area = aileronChord * aileronSpan;
                break;
            
            case "flap":
                area = flapChord * flapSpan;
                break;
            
            case "elevator":
                area = elevatorChord * elevatorSpan;
                break;
            
            case "rudder":
                area = rudderChord * rudderSpan;
                break;
        }

        return area;
    }
    
    public void OnStartEngine()
    {
        engineOn = !engineOn;
        if (engineOn && propellerSpinRate < 1) propellerSpinRate += 1f;

    }

}