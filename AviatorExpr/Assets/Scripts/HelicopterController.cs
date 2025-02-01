using System;
using UnityEngine;
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
    
    [FormerlySerializedAs("spinRate")]
    [Space]
    
    [Header("SpinAxis")]
    [SerializeField] 
    private float maxSpinRate = 200;
    
    [SerializeField, Tooltip("The torque to add per frame.")] 
    private float spinDeceleration = 1.2f;
    
    [SerializeField, Tooltip("The torque to add per frame.")] 
    private float spinAcceleration = 1.02f;
    
    private float currentSpinRate = 0;
    private float mainBladeSpeed = 0;
    private float tailBladeSpeed = 0;
    [SerializeField] private Vector3 mainSpinAxis = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 tailSpinAxis = new Vector3(0,0, 1);

    // In meters
    private float altitude = 0; 
    
    private bool engineOn;
    private Rigidbody tailBladeBody;
    private Rigidbody mainBladeBody;
    private Rigidbody heliBody;

    private float totalMass;

    
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heliBody = helicopter.GetComponent<Rigidbody>();
        heliBody.centerOfMass = centerMass.transform.localPosition;
        
        tailBladeBody = tailBlades.GetComponent<Rigidbody>();
        mainBladeBody = mainBlades.GetComponent<Rigidbody>();

        // Calculate mass
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            totalMass += rb.mass;
        }
        OnStartEngine();
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
    }

    private void Lift()
    {
        // Hovering
        // Thrust = ½ × Air Density × ( Rotor Radius × Rotor Angular Velocity)² × π × Rotor Radius² 
        double mainThrust = 0.5f * GetAirDensity(altitude) * GetBladeArea(mainBladeRadius) * Mathf.Pow(mainBladeSpeed, 2);
        heliBody.AddForceAtPosition(mainBladeBody.transform.up * (float)(mainThrust * .0049075f), mainBlades.transform.position);
        
        // Tail stabilisation
        double tailThrust = 0.5f * GetAirDensity(altitude) * GetBladeArea(tailBladeRadius) * Mathf.Pow(tailBladeSpeed, 2);
        heliBody.AddForceAtPosition(tailBladeBody.transform.forward * (float)(tailThrust * Mathf.Pow(.0049075f, 2)), tailBladeBody.transform.position);

        // Lift Equation: L = 0.5 * airDensity * velocity² * area * lift coefficient
    }
    
    private void SpinBlades()
    {
        if (!engineOn)
        {
            currentSpinRate -= currentSpinRate * spinDeceleration;
            currentSpinRate = Math.Max(0, currentSpinRate);
            
            tailBladeBody.AddRelativeTorque(tailSpinAxis * currentSpinRate); 
            mainBladeBody.AddRelativeTorque(mainSpinAxis * (currentSpinRate));
            // print(currentSpinRate);
            return;
        }

        currentSpinRate = Math.Min(currentSpinRate * spinAcceleration, maxSpinRate);
        tailBladeBody.AddRelativeTorque(tailSpinAxis * (5 * currentSpinRate)); 
        mainBladeBody.AddRelativeTorque(mainSpinAxis * (currentSpinRate));
        // print(currentSpinRate);
    }
    
    public void OnStartEngine()
    {
        print("hola");
        engineOn = !engineOn;
        if (engineOn && currentSpinRate < 1) currentSpinRate += 1f;
    }
    
    public void OnThrottleUp()
    {
       
    }
    
    public void OnThrottleDown()
    {
       
    }

    static float GetBladeArea(float radius)
    {
        return (float)(Math.PI * (radius * radius));
    }
    
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
