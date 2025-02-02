using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Objects")] 
    [SerializeField] private GameObject plane;

    [SerializeField] private GameObject rudder;

    [SerializeField] private GameObject elevator;

    [SerializeField] private GameObject[] flaps;

    [SerializeField] private GameObject[] ailerons;

    [SerializeField] private GameObject propeller;

    [SerializeField] private GameObject centerMass;

    private Rigidbody planeRb;
    private Rigidbody rudderRb;
    private Rigidbody elevatorRb;

    // left right
    private List<Rigidbody> flapRbs;
    private List<Rigidbody> aileronRbs;

    private Rigidbody propellerRb;


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
    private float maxPropellerDecelSpinRate = 100;
    
    private float propellerSpinRate;

    [SerializeField] 
    private Vector3 propellerSpinAxis = new(0, 1, 0);
    
    [SerializeField] 
    private float propellerSpinAccel = 1.02f;
    
    [SerializeField] 
    private float propellerSpinDecel = .9f;
    
    private float propellerSpeed;

    private InputController inputManager;

    [SerializeField]
    private float propellerPowerScaling = 1f;    
    private float altitude;

    
    private void Awake()
    {
        inputManager = GetComponent<InputController>();
        
        planeRb = plane.GetComponent<Rigidbody>();
                                                       
        rudderRb = rudder.GetComponent<Rigidbody>();
        elevatorRb = elevator.GetComponent<Rigidbody>();
        
        propellerRb = propeller.GetComponent<Rigidbody>();
        
        propellerRb.maxAngularVelocity = maxPropellerAccelSpinRate + 50;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planeRb.centerOfMass = centerMass.transform.localPosition;
        
        foreach (GameObject flap in flaps)
        {
            flapRbs.Add(flap.GetComponent<Rigidbody>());
        }
        
        foreach (GameObject aileron in ailerons)
        {
            aileronRbs.Add(aileron.GetComponent<Rigidbody>());
        }
        
    }

    // Update is called once per frame
    private void Update()
    {
        altitude = planeRb.transform.position.y;
        
        SpinPropeller();
        ThrottleControl();
        print(propellerSpinRate);
    }

    private void FixedUpdate()
    {
        Thrust(true);
    }

    private void ThrottleControl()
    {
        if (inputManager.throttleUpPressed) maxPropellerSpinRate = maxPropellerAccelSpinRate;
        else if (inputManager.throttleDownPressed) maxPropellerSpinRate = maxPropellerDecelSpinRate;
        else maxPropellerSpinRate = maxPropellerIdleSpinRate;
        
        // print("max: " + maxSpinRate);
        // print("current: " + mainBladeSpeed);
    }
    
    private void SpinPropeller()
    {
        propellerSpeed = propellerRb.angularVelocity.magnitude;
        
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            // Main Blade
            propellerSpinRate -= propellerSpinRate * propellerSpinDecel;
            propellerSpinRate = Math.Max(0, propellerSpinRate);
            propellerRb.AddRelativeTorque(propellerSpinAxis * (propellerSpinRate));
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        propellerSpinRate = Math.Min(propellerSpinRate * propellerSpinAccel, maxPropellerSpinRate);
        
        // Add torque... Clamp its angular velocity
        propellerRb.AddRelativeTorque(propellerSpinAxis * (propellerSpinRate));

        propellerRb.angularVelocity = Math.Min(propellerRb.angularVelocity.magnitude, maxPropellerSpinRate) * GetPropellerForwardAxis();
    }

    private void Thrust(bool invert)
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(propellerRadius) * Mathf.Pow(propellerSpeed, 2);
        planeRb.AddForceAtPosition((invert ? -GetPropellerForwardAxis() : GetPropellerForwardAxis()) * (float)(mainThrust * propellerPowerScaling), propellerRb.transform.position);
    }

    private Vector3 GetPropellerForwardAxis()
    {
        Vector3 localForward;
        if (propellerSpinAxis == Vector3.right) localForward = propellerRb.transform.right;
        else if (propellerSpinAxis == Vector3.up) localForward = propellerRb.transform.up;
        else localForward = propellerRb.transform.forward;

        return localForward;
    }

    public void OnStartEngine()
    {
        engineOn = !engineOn;
        if (engineOn && propellerSpinRate < 1) propellerSpinRate += 1f;

        print("hola");
    }

}