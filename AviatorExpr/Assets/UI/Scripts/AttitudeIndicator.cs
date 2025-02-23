/**************************************************************************************************************
* Attitude Indicator  
*
*   Sets the position and rotation of sections of the attitude indicator to make it represent the state of the aviator.  
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System;
using UnityEngine;

public class AttitudeIndicator : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField]
    private Transform playerTransform;
    
    [SerializeField]
    private RectTransform innerAttitude;
    
    [SerializeField]
    private RectTransform outerRoll;
    
    [SerializeField]
    private RectTransform bkg;

    [Header("Rotations")] 
    [SerializeField] 
    private float pitchMultiplier = 2f;
    
    [SerializeField] 
    private float rollMultiplier = 1;
    
    [SerializeField]
    private float pitchLimit = 20;
    
    void Update()
    {
        // Get aircraft rotation in world space
        Vector3 aircraftEuler = playerTransform.eulerAngles;
        
        // Convert to correct pitch (-180 to 180 range)
        float pitch = (aircraftEuler.x > 180) ? aircraftEuler.x - 360 : aircraftEuler.x;
        float yValue = Math.Clamp(pitch * pitchMultiplier, -pitchLimit, pitchLimit) ;
        
        // Set the roll rotation (Z-axis)
        innerAttitude.SetLocalPositionAndRotation(new(0,yValue,0), Quaternion.Euler(0, 0, -aircraftEuler.z * rollMultiplier));
        outerRoll.localRotation = Quaternion.Euler(0, 0, -aircraftEuler.z * rollMultiplier);
        bkg.SetLocalPositionAndRotation(new(0,yValue,0), Quaternion.Euler(0, 0, -aircraftEuler.z * rollMultiplier));
    }
}
