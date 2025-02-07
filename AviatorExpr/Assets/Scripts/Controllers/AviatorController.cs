using System;
using UnityEngine;

public class AviatorController : MonoBehaviour
{
    [SerializeField] 
    protected GameObject mainObject;
    
    [SerializeField] 
    protected GameObject mainPropeller;
   
    [SerializeField] 
    protected GameObject centerMass;

    protected Rigidbody mainRb;
    protected Rigidbody mainPropellerRb;

    
    ///////////// Main Propeller
    [Header("Main Propeller")]
    [SerializeField] 
    protected Vector3 mainPropellerSpinAxis = new(0, 1, 0);
    
    [SerializeField, Tooltip("The radius of the main propeller.")]

    // The current spin rate
    protected float mainPropellerSpinRate;
    protected float maxPropellerSpinRate;
    
    
    ///////////// Controls
    [SerializeField]
    protected bool invertPitch = true;
    protected InputController inputManager;

    [SerializeField] 
    protected AviatorStats stats;

    protected bool engineOn;

    protected float altitude;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ThrottleControl();
    }

    private void FixedUpdate()
    {
        SpinPropeller();
        Lift();
        
        RollControl();
        PitchControl();
        YawControl();
    }

    protected void ThrottleControl()
    {
        
    }
    
    protected void SpinPropeller()
    {
        
    }
    
    protected void Lift()
    {
        
    }
    
    protected void YawControl()
    {
        
    }
    
    protected void PitchControl()
    {
        
    }
    
    protected void RollControl()
    {
        
    }
}
