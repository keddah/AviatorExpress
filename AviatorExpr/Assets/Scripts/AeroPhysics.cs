/**************************************************************************************************************
* Aero Physics 
*
*   Contains static functions that are used in the aerodynamics sections of the aviator classes.  
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System;
using UnityEngine;

static class AeroPhysics
{
    public static float GetBladeArea(float radius) { return (float)(Math.PI * (radius * radius)); }
    
    public static float GetAirDensity(float altitude)
    {
        // Sea-level temperature(Kelvin)  Temperature drop per meter (K/m)
        const float seaLvlTemp = 288.15f, deltaTemp = 0.0065f; 
        
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
        return totalWingArea / (wingSectionCount * 2);
    }
}