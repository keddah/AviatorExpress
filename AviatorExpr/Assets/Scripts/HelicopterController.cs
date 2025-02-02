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
    
    [FormerlySerializedAs("maxHoverSpinRate")] [SerializeField] 
    private float maxMainHoverSpinRate = 30;
    
    [FormerlySerializedAs("maxDecelerationSpinRate")] [SerializeField] 
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
    
    [Header("SpinAxis")]
    [SerializeField] private Vector3 mainSpinAxis = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 tailSpinAxis = new Vector3(0,0, 1);

    
    [Header("Power")]
    [SerializeField]
    private const float mainBladePowerScaling = .01f;
    
    [SerializeField]
    private const float tailBladePowerScaling = .01f;
    
    // In meters
    private float altitude = 0; 
    
    private bool engineOn;
    private Rigidbody tailBladeBody;
    private Rigidbody mainBladeBody;
    private Rigidbody heliBody;

    private float totalMass;

    private InputController inputManager;


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
        
        tailBladeBody = tailBlades.GetComponent<Rigidbody>();
        mainBladeBody = mainBlades.GetComponent<Rigidbody>();

        // Set the max angular velocity (wouldn't need to be that much bigger than the max acceleration spin rate)
        mainBladeBody.maxAngularVelocity = maxMainAccelSpinRate + 50;
        tailBladeBody.maxAngularVelocity = maxTailAccelSpinRate + 50;
        
        // Calculate mass
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            totalMass += rb.mass;
        }
    }
    
    private void FixedUpdate()
    {
        SpinBlades();
        Lift();
    }

    // Update is called once per frame
    void Update()
    {
        mainBladeSpeed = mainBladeBody.angularVelocity.magnitude;
        tailBladeSpeed = tailBladeBody.angularVelocity.magnitude;
        altitude = helicopter.transform.position.y;
    
        print(maxMainSpinRate);
        ThrottleControl();
        TailSteering();
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
        double mainThrust = 0.5f * GetAirDensity(altitude) * GetBladeArea(mainBladeRadius) * Mathf.Pow(mainBladeSpeed, 2);
        heliBody.AddForceAtPosition(mainBladeBody.transform.up * (float)(mainThrust * mainBladePowerScaling), mainBlades.transform.position);
        
        // Tail stabilisation
        double tailThrust = (0.5f * GetAirDensity(altitude) * GetBladeArea(tailBladeRadius) * Mathf.Pow(tailBladeSpeed, 2)) * (turningLeft ? -1 : 1);
        heliBody.AddForceAtPosition(tailBladeBody.transform.forward * (float)(tailThrust * tailBladePowerScaling), tailBladeBody.transform.position);

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
            
            tailBladeBody.AddRelativeTorque(tailSpinAxis * currentTailSpinRate); 
            mainBladeBody.AddRelativeTorque(mainSpinAxis * (currentMainSpinRate));
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        currentMainSpinRate = Math.Min(currentMainSpinRate * spinAcceleration, maxMainSpinRate);
        currentTailSpinRate = Math.Min(currentTailSpinRate * spinAcceleration, maxTailSpinRate);
        
        // Add torque... Clamp its angular velocity
        mainBladeBody.AddRelativeTorque(mainSpinAxis * (currentMainSpinRate));
        mainBladeBody.angularVelocity = Math.Min(mainBladeBody.angularVelocity.magnitude, maxMainSpinRate) * mainBladeBody.transform.up;
        
        tailBladeBody.AddRelativeTorque(tailSpinAxis * currentTailSpinRate); 
        tailBladeBody.angularVelocity = Math.Clamp(tailBladeBody.angularVelocity.magnitude, -maxTailSpinRate, maxTailSpinRate) * tailBladeBody.transform.forward;
        // print(tailBladeBody.angularVelocity);
    }
    
    
    
    
    public void OnStartEngine()
    {
        engineOn = !engineOn;
        if (engineOn && currentMainSpinRate < 1) currentMainSpinRate += 1f;
        if (engineOn && currentTailSpinRate < 1) currentTailSpinRate += 1f;
    }
    
    static float GetBladeArea(float radius) { return (float)(Math.PI * (radius * radius)); }
    
    static float GetAirDensity(float altitude)
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
}
