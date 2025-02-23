/**************************************************************************************************************
* Aviator Stats  
*
*   A container for all the properties that aviators need to work.
*   Depending on the aviator type, some fields are hidden. ~ Using the EditorScript class 
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "AviatorStats", menuName = "Scriptable Objects/AviatorStats")]
public class AviatorStats : ScriptableObject
{
    public enum EAviatorType
    {
        Helicopter,
        Plane,
        Blimp,
        None
    };

    public EAviatorType aviationType;

    [Header("Propeller")]
    public float mainPropellerRadius = 14; 
    
    public uint maxMainPropellerAccelSpinRate = 300;
    public uint maxMainPropellerIdleSpinRate = 200;
    public uint maxMainPropellerDecelSpinRate = 150;

    public float mainPropellerSpinAccel = 1.02f;
    public float mainPropellerSpinDecel = .09f;
    
    public float propellerPowerScaling = .1f;    

    /////////////// Hidden in HELICOPTER enum ///////////////
    // Spin Rates
    [HideInInspector] public bool showTailPropeller = true;
    public float tailPropellerRadius = .8f;
    public float tailPropellerSpinAccel = 1.02f;
    public float tailPropellerSpinDecel = .09f;
    
    public uint maxTailPropellerAccelSpinRate = 600;
    public uint maxTailPropellerIdleSpinRate = 30;
    
    
    // Gyro
    [HideInInspector] public bool showGyro = true;
    public uint gyroPower = 2000;
    public float gyroAssistStrength = .8f;
    public uint stabilisationStrength = 1000;
    public float rollDamping = .4f;
    /////////////////////////////////////////////////////////
    
    /////////////// Hidden in PLANE enum ////////////////////
    public float liftCoefficient = 1.5f;
    public float wingspan = 14;
    
    // Angles
    [HideInInspector] public bool showWingSectionAngles = true;
    public float angleDampener = .75f;
    public ushort maxAileronAngle = 75;
    public ushort maxElevatorAngle = 45;
    public ushort maxFlapAngle = 80;
    public short minFlapAngle = -60;
    public ushort maxRudderAngle = 60;
    
    // Speeds
    [HideInInspector] public bool showWingSectionSpeeds = true;
    public float aileronSpeed = 1.2f;
    public float elevatorSpeed = 1.35f;
    public float flapSpeed = 7.5f;
    public float rudderSpeed = 5;
    
    // To Neutral Speeds
    [HideInInspector] public bool showWingSectionNeutralSpeeds = true;
    public float aileronToNeutralSpeed = 6;
    public float elevatorToNeutralSpeed = 3;
    public float flapToNeutralSpeed = 3;
    public float rudderToNeutralSpeed = 6;
    
    // Spans
    [HideInInspector] public bool showWingSpans = true;
    public float aileronSpan = .6f;
    public float elevatorSpan = 3.8f;
    public float flapSpan = .6f;
    public float rudderSpan = 1.8f;
    
    // Chords
    [HideInInspector] public bool showWingChords = true;
    public float rudderChord = .4f;
    public float elevatorChord = .7f;
    public float aileronChord = .3f;
    public float flapChord = .3f;
    /////////////////////////////////////////////////////////
}