/**************************************************************************************************************
* Editor Script  
*
*   Only used to show/hide properties for the aviator stats scriptable objects depending on the type of aviator.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AviatorStats))]
public class EditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        AviatorStats aviator = (AviatorStats)target;
        aviator.aviationType = (AviatorStats.EAviatorType)EditorGUILayout.EnumPopup("Aviator Type", aviator.aviationType);

        // ALWAYS SHOW
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Propeller", EditorStyles.boldLabel);
        aviator.mainPropellerRadius = EditorGUILayout.FloatField(new GUIContent("Propeller radius", "The radius of the main propeller."), aviator.mainPropellerRadius);
        aviator.propellerPowerScaling = EditorGUILayout.FloatField("Propeller Power Scaling", aviator.propellerPowerScaling);
        EditorGUILayout.Space();
        
        aviator.maxMainPropellerAccelSpinRate = (uint)EditorGUILayout.IntField("Max Acceleration Spin Rate", (int)aviator.maxMainPropellerAccelSpinRate);
        aviator.maxMainPropellerIdleSpinRate = (uint)EditorGUILayout.IntField("Max Idle Spin Rate", (int)aviator.maxMainPropellerIdleSpinRate);
        aviator.maxMainPropellerDecelSpinRate = (uint)EditorGUILayout.IntField("Max Deceleration Spin Rate", (int)aviator.maxMainPropellerDecelSpinRate);
        EditorGUILayout.Space();
        
        aviator.mainPropellerSpinAccel = EditorGUILayout.FloatField("Propeller Acceleration Rate", aviator.mainPropellerSpinAccel);
        aviator.mainPropellerSpinDecel = EditorGUILayout.FloatField("Propeller Deceleration Rate", aviator.mainPropellerSpinDecel);
        EditorGUILayout.Space();
        
        
        // SHOW ON SWITCH
        switch (aviator.aviationType)
        {
            ///// HELICOPTER
            case AviatorStats.EAviatorType.Helicopter:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Helicopter Settings", EditorStyles.boldLabel);
                
                aviator.showTailPropeller = EditorGUILayout.Foldout(aviator.showTailPropeller, "Tail Propeller", true);
                if (aviator.showTailPropeller)
                {
                    EditorGUI.indentLevel++;
                    aviator.tailPropellerRadius = EditorGUILayout.FloatField("Tail Propeller Radius", aviator.tailPropellerRadius);
                    aviator.tailPropellerSpinAccel = EditorGUILayout.FloatField("Tail Propeller Acceleration", aviator.tailPropellerSpinAccel);
                    aviator.tailPropellerSpinDecel = EditorGUILayout.FloatField("Tail Propeller Deceleration", aviator.tailPropellerSpinDecel);
                    EditorGUILayout.Space();
                    
                    aviator.maxTailPropellerAccelSpinRate = (uint)EditorGUILayout.IntField("Max Tail Propeller Acceleration Spin Rate", (int)aviator.maxTailPropellerAccelSpinRate);
                    aviator.maxTailPropellerIdleSpinRate = (uint)EditorGUILayout.IntField("Max Tail Propeller Idle Spin Rate", (int)aviator.maxTailPropellerIdleSpinRate);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                aviator.showGyro = EditorGUILayout.Foldout(aviator.showGyro, "Gyro Control", true);
                if (aviator.showGyro)
                {
                    EditorGUI.indentLevel++;
                    aviator.gyroPower = (uint)EditorGUILayout.IntField("Gyro Power", (int)aviator.gyroPower);
                    
                    aviator.gyroAssistStrength = 
                        EditorGUILayout.FloatField(new GUIContent("Gyro Assist Strength","The strength of the easing when moving."), aviator.gyroAssistStrength);
                    aviator.stabilisationStrength = 
                        (uint)EditorGUILayout.IntField(new GUIContent("Stabilisation Strength", "The strength of keeping the Helicopter upright"), (int)aviator.stabilisationStrength);
                    
                    aviator.rollDamping = EditorGUILayout.FloatField(new GUIContent("Roll Damping", "Multiplier to be applied to the roll gyro control."), aviator.rollDamping);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel--;
                }
                break;

            
            ///// PLANE
            case AviatorStats.EAviatorType.Plane:
                EditorGUILayout.LabelField("Plane Settings", EditorStyles.boldLabel);
                aviator.liftCoefficient = EditorGUILayout.FloatField("Lift Coefficient", aviator.liftCoefficient);
                aviator.wingspan = EditorGUILayout.FloatField("Wingspan", aviator.wingspan);
                EditorGUILayout.Space();
                
                aviator.showWingSectionAngles = EditorGUILayout.Foldout(aviator.showWingSectionAngles, "Wing Section Angles", true);
                if (aviator.showWingSectionAngles)
                {
                    EditorGUI.indentLevel++;
                    aviator.angleDampener = EditorGUILayout.FloatField(new GUIContent("Angle Dampening", 
                        "Multiplier to be applied to the angle of attack to make the angle of attack more/less of a factor for plane sections"), aviator.angleDampener);
                    
                    aviator.maxAileronAngle = (ushort)EditorGUILayout.IntField("Max Aileron Angle", aviator.maxAileronAngle);
                    aviator.maxElevatorAngle = (ushort)EditorGUILayout.IntField("Max Elevator Angle", aviator.maxElevatorAngle);
                    aviator.maxFlapAngle = (ushort)EditorGUILayout.IntField("Max Flap Angle", aviator.maxFlapAngle);
                    aviator.minFlapAngle = (short)EditorGUILayout.IntField("Min Flap Angle", aviator.minFlapAngle);
                    aviator.maxRudderAngle = (ushort)EditorGUILayout.IntField("Max Rudder Angle", aviator.maxRudderAngle);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                
                aviator.showWingSectionSpeeds = EditorGUILayout.Foldout(aviator.showWingSectionSpeeds, "Wing Section Speeds", true);
                if (aviator.showWingSectionSpeeds)
                {
                    EditorGUI.indentLevel++;
                    aviator.aileronSpeed = EditorGUILayout.FloatField("Aileron Speed", aviator.aileronSpeed);
                    aviator.elevatorSpeed = EditorGUILayout.FloatField("Elevator Speed", aviator.elevatorSpeed);
                    aviator.flapSpeed = EditorGUILayout.FloatField("Flap Speed", aviator.flapSpeed);
                    aviator.rudderSpeed = EditorGUILayout.FloatField("Rudder Speed", aviator.rudderSpeed);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                
                aviator.showWingSectionNeutralSpeeds = EditorGUILayout.Foldout(aviator.showWingSectionNeutralSpeeds, "Wing Section To Neutral Speeds", true);
                if (aviator.showWingSectionNeutralSpeeds)
                {
                    EditorGUI.indentLevel++;
                    aviator.aileronToNeutralSpeed = EditorGUILayout.FloatField("Aileron To Neutral Speed", aviator.aileronToNeutralSpeed);
                    aviator.elevatorToNeutralSpeed = EditorGUILayout.FloatField("Elevator To Neutral Speed", aviator.elevatorToNeutralSpeed);
                    aviator.flapToNeutralSpeed = EditorGUILayout.FloatField("Flap To Neutral Speed", aviator.flapToNeutralSpeed);
                    aviator.rudderToNeutralSpeed = EditorGUILayout.FloatField("Rudder To Neutral Speed", aviator.rudderToNeutralSpeed);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                
                aviator.showWingSpans = EditorGUILayout.Foldout(aviator.showWingSpans, "Wing Section Spans", true);
                if (aviator.showWingSpans)
                {
                    EditorGUI.indentLevel++;
                    aviator.aileronSpan = EditorGUILayout.FloatField("Aileron Span", aviator.aileronSpan);
                    aviator.elevatorSpan = EditorGUILayout.FloatField("Elevator Span", aviator.elevatorSpan);
                    aviator.flapSpan = EditorGUILayout.FloatField("Flap Span", aviator.flapSpan);
                    aviator.rudderSpan = EditorGUILayout.FloatField("Rudder Span", aviator.rudderSpan);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                
                aviator.showWingChords = EditorGUILayout.Foldout(aviator.showWingChords, "Wing Section Chords", true);
                if (aviator.showWingChords)
                {
                    EditorGUI.indentLevel++;
                    aviator.aileronChord = EditorGUILayout.FloatField("Aileron Span", aviator.aileronChord);
                    aviator.elevatorChord = EditorGUILayout.FloatField("Elevator Span", aviator.elevatorChord);
                    aviator.flapChord = EditorGUILayout.FloatField("Flap Span", aviator.flapChord);
                    aviator.rudderChord = EditorGUILayout.FloatField("Rudder Span", aviator.rudderChord);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                break;

            case AviatorStats.EAviatorType.Blimp:
                EditorGUILayout.LabelField("Blimp Settings", EditorStyles.boldLabel);
                break;
        }

        EditorUtility.SetDirty(aviator);
    }
}