using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class HelicopterController : MonoBehaviour
{
    [SerializeField] private GameObject centerMass;
    [SerializeField] private GameObject helicopter;
    
    [Space]
    
    [Header("Blade Objects")]
    [SerializeField] private GameObject mainBlades;
    [SerializeField] private GameObject tailBlades;
    
    [SerializeField] private float mainBladeRadius = 22.5f;
    [SerializeField] private float tailBladeRadius = 5f;
    
    [FormerlySerializedAs("maxAccelerationSpinRate")]
    [FormerlySerializedAs("maxSpinRate")]
    [FormerlySerializedAs("spinRate")]
    [Space]
    
    [Header("Spin Rate")]
    [SerializeField] 
    private float maxMainAccelSpinRate = 60;
    
    [SerializeField] 
    private float maxMainHoverSpinRate = 30;
    
    [SerializeField] 
    private float maxMainDecelSpinRate = 5;
    
    [Header("Spin Rate")]
    [SerializeField] 
    private float maxTailAccelSpinRate = 60;
    
    [SerializeField] 
    private float maxTailHoverSpinRate = 20;
    
    private float maxMainSpinRate;
    private float maxTailSpinRate;
    private bool turningLeft;
    
    [SerializeField, Tooltip("The torque to multiplied per frame.")] 
    private float spinDeceleration = 1.2f;
    
    [SerializeField, Tooltip("The torque to be multiplied per frame.")] 
    private float spinAcceleration = 1.02f;
    
    private float currentMainSpinRate = 0;
    private float currentTailSpinRate = 0;
    private float mainBladeSpeed = 0;
    private float tailBladeSpeed = 0;
    
    [Header("Axis")]
    [SerializeField] private Vector3 copterUpAxis = new Vector3(1, 0, 0);
    [SerializeField] private Vector3 mainSpinAxis = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 tailSpinAxis = new Vector3(0,0, 1);

    
    [Header("Power")]
    private const float bladePowerScaling = .01f;
    
    // In meters
    private float altitude = 0; 
    
    private bool engineOn;
    private Rigidbody tailBladeRb;
    private Rigidbody mainBladeRb;
    private Rigidbody heliBody;

    private float totalMass;

    private InputController inputManager;

    [Header("Gyro Control")]
    [SerializeField]
    private float gyroPower = 200;
    
    [SerializeField]
    private float gyroAssistStrength = .2f;
    
    [SerializeField]
    private float stabilisationStrength = 2000;

    private void Awake()
    {
        inputManager = GetComponent<InputController>();
        maxMainSpinRate = maxTailHoverSpinRate;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heliBody = helicopter.GetComponent<Rigidbody>();
        heliBody.centerOfMass = centerMass.transform.localPosition;
        
        tailBladeRb = tailBlades.GetComponent<Rigidbody>();
        mainBladeRb = mainBlades.GetComponent<Rigidbody>();

        // Set the max angular velocity (wouldn't need to be that much bigger than the max acceleration spin rate)
        mainBladeRb.maxAngularVelocity = maxMainAccelSpinRate + 50;
        tailBladeRb.maxAngularVelocity = maxTailAccelSpinRate + 50;
        
        // Calculate mass
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            totalMass += rb.mass;
        }
    }
    
    private void FixedUpdate()
    {
        GyroControl();
        SpinBlades();
        Lift();
    }

    // Update is called once per frame
    void Update()
    {
        mainBladeSpeed = mainBladeRb.angularVelocity.magnitude;
        tailBladeSpeed = tailBladeRb.angularVelocity.magnitude;
        altitude = helicopter.transform.position.y;
    
        ThrottleControl();
        TailSteering();
    }

    private void Stabiliser()
    {
        Vector3 localUp = new();
        if (copterUpAxis == Vector3.right) localUp = heliBody.transform.right;
        else if (copterUpAxis == Vector3.up) localUp = heliBody.transform.up;
        else if (copterUpAxis == Vector3.forward) localUp = heliBody.transform.forward;
        
        Vector3 correctionVector = Vector3.Cross(localUp,Vector3.up);
        heliBody.AddTorque(correctionVector * stabilisationStrength);
    }
    
    private void GyroAssist()
    {
        // Constantly fight against the angular velocity to try to stabilise
        heliBody.angularVelocity = Vector3.Lerp(heliBody.angularVelocity, Vector3.zero, Time.deltaTime * gyroAssistStrength);
        
        Stabiliser();
    }
    
    private void GyroControl()
    {
        Vector2 input = inputManager.moveInput;

        Vector3 velocityToAdd = new();
        velocityToAdd.x = input.x * gyroPower;
        velocityToAdd.z = -input.y * gyroPower;
        heliBody.AddRelativeTorque(velocityToAdd);
        
        GyroAssist();
    }
    
    private void TailSteering()
    {
        turningLeft = inputManager.leftPressed;
        
        // Same max speed
        // Flip the direction in SpinBlades()
        if (inputManager.rightPressed || turningLeft) maxTailSpinRate = maxTailAccelSpinRate;
        else maxTailSpinRate = maxTailHoverSpinRate;
    }
    
    private void ThrottleControl()
    {
        if (inputManager.throttleUpPressed) maxMainSpinRate = maxMainAccelSpinRate;
        else if (inputManager.throttleDownPressed) maxMainSpinRate = maxMainDecelSpinRate;
        else maxMainSpinRate = maxMainHoverSpinRate;
        
        // print("max: " + maxSpinRate);
        // print("current: " + mainBladeSpeed);
    }
    
    private void Lift()
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(mainBladeRadius) * Mathf.Pow(mainBladeSpeed, 2);
        heliBody.AddForceAtPosition(mainBladeRb.transform.up * (float)(mainThrust * bladePowerScaling), mainBlades.transform.position);
        
        // Tail stabilisation
        double tailThrust = (0.5f * AeroPhysics.GetAirDensity(altitude) * AeroPhysics.GetBladeArea(tailBladeRadius) * Mathf.Pow(tailBladeSpeed, 2)) * (turningLeft ? 1 : -1);
        heliBody.AddForceAtPosition(-tailBladeRb.transform.forward * (float)(tailThrust * bladePowerScaling), tailBladeRb.transform.position);

        // Lift Equation: L = 0.5 * airDensity * velocity² * area * lift coefficient
    }
    
    private void SpinBlades()
    {
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            // Main Blade
            currentMainSpinRate -= currentMainSpinRate * spinDeceleration;
            currentMainSpinRate = Math.Max(0, currentMainSpinRate);

            // Tail blade
            currentTailSpinRate -= currentTailSpinRate * spinDeceleration;
            currentTailSpinRate = Math.Max(0, currentTailSpinRate);
            
            tailBladeRb.AddRelativeTorque(tailSpinAxis * currentTailSpinRate); 
            mainBladeRb.AddRelativeTorque(mainSpinAxis * (currentMainSpinRate));
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        currentMainSpinRate = Math.Min(currentMainSpinRate * spinAcceleration, maxMainSpinRate);
        currentTailSpinRate = Math.Min(currentTailSpinRate * spinAcceleration, maxTailSpinRate);
        
        // Add torque... Clamp its angular velocity
        mainBladeRb.AddRelativeTorque(mainSpinAxis * (currentMainSpinRate));
        mainBladeRb.angularVelocity = Math.Min(mainBladeRb.angularVelocity.magnitude, maxMainSpinRate) * mainBladeRb.transform.up;
        
        tailBladeRb.AddRelativeTorque(tailSpinAxis * currentTailSpinRate); 
        tailBladeRb.angularVelocity = Math.Clamp(tailBladeRb.angularVelocity.magnitude, -maxTailSpinRate, maxTailSpinRate) * tailBladeRb.transform.forward;
        // print(tailBladeBody.angularVelocity);
    }
    
    
    
    
    public void OnStartEngine()
    {
        engineOn = !engineOn;
        if (engineOn && currentMainSpinRate < 1) currentMainSpinRate += 1f;
        if (engineOn && currentTailSpinRate < 1) currentTailSpinRate += 1f;
    }
    
}

class AeroPhysics
{
    public static bool QuatApproximately(Quaternion a, Quaternion b)
    {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
    }
    
    public static float GetBladeArea(float radius) { return (float)(Math.PI * (radius * radius)); }
    
    public static float GetAirDensity(float altitude)
    {
        // Sea-level temperature (Kelvin)
        const float seaLvlTemp = 288.15f; 
        
        // Temperature drop per meter (K/m)
        const float deltaTemp = 0.0065f; 
        
        // Specific gas constant for air (J/kg·K)
        const float gasConstant = 287.05f; 

        // Temperature at altitude (Kelvin)
        float tempAtAltitude = seaLvlTemp - deltaTemp * altitude;

        const float seaLvlPressure = 101325f;
        
        // Pressure at altitude (using barometric formula)
        float pressureAtAltitude = seaLvlPressure * Mathf.Pow((tempAtAltitude / seaLvlTemp), (-Physics.gravity.y / (deltaTemp * gasConstant)));

        // Air density using Ideal Gas Law: ρ = P / (R * T)
        return pressureAtAltitude / (gasConstant * tempAtAltitude);;
    }

    // Total wingspan (meters)
    // root chord = Width of wing at the center of the plane
    // tip chord = Width of the wing at the end of the wing
    // Number of sections per wing
    public static float FindWingAreaPerSection(float wingSpan = 14, float rootChord = 1.8f, float tipChord = 1.5f, float wingSectionCount = 2)
    {
        float totalWingArea = wingSpan * ((rootChord + tipChord) / 2);
        
        // Multiply by 2 since there are two wings
        return   totalWingArea / (wingSectionCount * 2);
    }
    
    public static float FindWingSectionArea(float wingSpan, float chordLength)
    {
        return wingSpan * chordLength;
    }
}