using System;
using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    [SerializeField] private float spinRate = 100;
    [SerializeField] private bool engineOn = false;
    
    [SerializeField] private GameObject mainBlades;
    [SerializeField] private GameObject tailBlades;
    private Rigidbody tailBladeBody;
    private Rigidbody mainBladeBody;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tailBladeBody = tailBlades.GetComponent<Rigidbody>();
        mainBladeBody = mainBlades.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!engineOn)
        {
            print("off");
            return;
        };
        tailBladeBody.AddRelativeTorque(0, spinRate, 0); 
        mainBladeBody.AddRelativeTorque(0, spinRate * 9000000000000000000, 0); 
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    private void OnStartEngine()
    {
        engineOn = !engineOn;
        print(engineOn);
    }
    
    private void OnThrottleUp()
    {
       
    }
    
    private void OnThrottleDown()
    {
       
    }
}
