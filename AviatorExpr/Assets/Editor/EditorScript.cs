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
        
        aviator.maxMainPropellerAccelSpinRate = EditorGUILayout.FloatField("Max Acceleration Spin Rate", aviator.maxMainPropellerAccelSpinRate);
        aviator.maxMainPropellerIdleSpinRate = EditorGUILayout.FloatField("Max Idle Spin Rate", aviator.maxMainPropellerIdleSpinRate);
        aviator.maxMainPropellerDecelSpinRate = EditorGUILayout.FloatField("Max Deceleration Spin Rate", aviator.maxMainPropellerDecelSpinRate);
        EditorGUILayout.Space();
        
        aviator.mainPropellerSpinAccel = EditorGUILayout.FloatField("Propeller Acceleration Rate", aviator.mainPropellerSpinAccel);
        aviator.mainPropellerSpinDecel = EditorGUILayout.FloatField("Propeller Deceleration Rate", aviator.mainPropellerSpinDecel);
        EditorGUILayout.Space();
        
        
        // Show on enum switch
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
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                aviator.showGyro = EditorGUILayout.Foldout(aviator.showGyro, "Gyro Control", true);
                if (aviator.showGyro)
                {
                    EditorGUI.indentLevel++;
                    aviator.gyroPower = EditorGUILayout.FloatField("Gyro Power", aviator.gyroPower);
                    aviator.gyroAssistStrength = EditorGUILayout.FloatField(new GUIContent("Gyro Assist Strength","The strength of the easing when moving."), aviator.gyroAssistStrength);
                    aviator.stabilisationStrength = EditorGUILayout.FloatField(new GUIContent("Stabilisation Strength", "The strength of keeping the Helicopter upright"), aviator.stabilisationStrength);
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
                    aviator.maxAileronAngle = EditorGUILayout.FloatField("Max Aileron Angle", aviator.maxAileronAngle);
                    aviator.maxElevatorAngle = EditorGUILayout.FloatField("Max Elevator Angle", aviator.maxElevatorAngle);
                    aviator.maxFlapAngle = EditorGUILayout.FloatField("Max Flap Angle", aviator.maxFlapAngle);
                    aviator.minFlapAngle = EditorGUILayout.FloatField("Min Flap Angle", aviator.minFlapAngle);
                    aviator.maxRudderAngle = EditorGUILayout.FloatField("Max Rudder Angle", aviator.maxRudderAngle);
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
                // aviator.maxPropellerIdleSpinRate = EditorGUILayout.FloatField("Max Idle Spin Rate", aviator.maxPropellerIdleSpinRate);
                break;
        }

        EditorUtility.SetDirty(aviator);
    }
}