using System;
using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    private AviatorController player;
    private HoopScript targetHoop;

    [SerializeField] 
    private float lookSpeed = 3;
    
    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetHoop = FindAnyObjectByType<HoopManager>().GetCurrentHoop();
    }

    void FixedUpdate()
    {
        Quaternion lookRot = Quaternion.LookRotation(targetHoop.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }
}
