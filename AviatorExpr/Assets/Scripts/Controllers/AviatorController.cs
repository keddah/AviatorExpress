/**************************************************************************************************************
* Aviator Controller
* 
*   The parent class to the player vehicles. Only outlines the common parameters and functions that will be used by each aviator type.
*   Invokes delegates for when the player respawns at hoops, pauses the game and starts a race.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System;
using Unity.Cinemachine;
using UnityEngine;

public class AviatorController : MonoBehaviour
{
    public event OnRespawnAtHoop onRetry;
    public delegate void OnRespawnAtHoop();
    
    public event OnGamePaused onGamePaused;
    public delegate void OnGamePaused();
    
    public event OnStartRace onRaceStart;
    public delegate void OnStartRace(ushort numHoops);
    
    [SerializeField] 
    protected AviatorStats stats;

    [Space] 
    [SerializeField]
    protected GameObject mainObject;
    
    [SerializeField] 
    protected GameObject mainPropeller;
   
    [SerializeField] 
    protected GameObject centerMass;

    protected Rigidbody mainRb { get; private set; }
    protected Rigidbody mainPropellerRb;

    [Space] 
    [SerializeField]
    public Vector3 bodyForwardAxis = new(0,0,1);
    [SerializeField]
    private Vector3 bodyUpAxis = new(0,1,0);
    
    
    ///////////// Main Propeller
    [SerializeField]
    protected bool invertPitch = true;
    
    [Space]
    
    [Header("Main Propeller")]
    [SerializeField] 
    protected Vector3 mainPropellerSpinAxis = new(0, 1, 0);
    
    private float maxPropellerSpinRate;
    
    // The current spin rate
    private float mainPropellerSpinRate;

    // The current propeller speed
    protected float mainPropellerSpeed;

    
    ///////////// Controls
    protected InputController inputManager;
    
    
    ///////////// Audio
    private AudioManager sfxManager;
    
    
    ///////////// Camera
    private bool lookingAtHoop;
    private CinemachineCamera cam;
    CinemachineTargetGroup.Target hoopGroupTarget;


    protected bool engineOn { get; private set; }
    protected float altitude;


    [Space] 
    private UIManager uiManager;
    
    [SerializeField] 
    private ushort scoreTimeBonus = 25;
    [Space]
    
    private ScoreManager scoreManager;

    protected virtual void Awake()
    {
        scoreManager = new(scoreTimeBonus);
        
        inputManager = GetComponent<InputController>();
        sfxManager = GetComponent<AudioManager>();
    
        mainRb = mainObject.GetComponent<Rigidbody>();
        mainRb.interpolation = RigidbodyInterpolation.Interpolate;
        mainPropellerRb = mainPropeller.GetComponent<Rigidbody>();
        mainPropellerRb.maxAngularVelocity = stats.maxMainPropellerAccelSpinRate + 5;

        mainRb.centerOfMass = centerMass.transform.localPosition;

        HingeJoint propellerHinge = mainPropeller.GetComponent<HingeJoint>();
        propellerHinge.axis = mainPropellerSpinAxis;

        uiManager = GetComponentInChildren<UIManager>(true);
        
        cam = GetComponentInChildren<CinemachineCamera>();
        
        SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        // Show the mouse then the race ends
        scoreManager.onEndRace += inputManager.UnlockMouse;
        
        ///// Setup hoop camera
        // Get the average orbit radius of the default camera.
        Cinemachine3OrbitRig.Settings orbitSettings = cam.GetComponent<CinemachineOrbitalFollow>().Orbits;
        float groupRadius = (orbitSettings.Top.Radius + orbitSettings.Center.Radius + orbitSettings.Bottom.Radius) / 3;
        
        // Setting this up for when the camera is toggled
        hoopGroupTarget = new CinemachineTargetGroup.Target { Object = FindAnyObjectByType<HoopManager>().GetCurrentHoop().transform, Radius = groupRadius };
        
        // Setting the group radius as the average orbit radius of the default camera.
        CinemachineTargetGroup targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
        targetGroup.Targets[0].Radius = groupRadius;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        scoreManager.Update();
        
        mainPropellerSpeed = mainPropellerRb.angularVelocity.magnitude;
        altitude = GetPosition().y;
        
        ThrottleControl();
        
        // print($"current hold: {inputManager.currenetRespawnHoldTime}");
        // print($"hold time: {inputManager.respawnActionHoldTime}");
        uiManager.ShowHideRespawning(inputManager.currenetRespawnHoldTime, inputManager.respawnActionHoldTime);
    }

    protected virtual void FixedUpdate()
    {
        SpinPropeller();
        Lift();
        
        RollControl();
        PitchControl();
        YawControl();
    }

    private void ThrottleControl()
    {
        // Accelerating
        if (inputManager.throttleUpValue > 0) maxPropellerSpinRate = stats.maxMainPropellerAccelSpinRate * inputManager.throttleUpValue;
        
        // Decelerating
        else if (inputManager.throttleDownValue > 0) maxPropellerSpinRate = stats.maxMainPropellerDecelSpinRate;
        
        // Idle
        else maxPropellerSpinRate = stats.maxMainPropellerIdleSpinRate;
    }
    
    protected virtual void SpinPropeller()
    {
        // Decelerate the blades whenever the engine is off
        if (!engineOn)
        {
            // Main Blade
            mainPropellerSpinRate -= mainPropellerSpinRate * stats.mainPropellerSpinDecel;
            mainPropellerSpinRate = Math.Max(0, mainPropellerSpinRate);

            mainPropellerRb.AddRelativeTorque(mainPropellerSpinAxis * mainPropellerSpinRate);
            return;
        }

        // Accelerate the blades.
        // Clamp the max spin speed
        mainPropellerSpinRate = Math.Min(mainPropellerSpinRate * stats.mainPropellerSpinAccel, maxPropellerSpinRate);
        
        // Add torque... Clamp its angular velocity
        mainPropellerRb.AddRelativeTorque(mainPropellerSpinAxis * (mainPropellerSpinRate));
        mainPropellerRb.angularVelocity = Math.Min(mainPropellerRb.angularVelocity.magnitude, maxPropellerSpinRate) * GetPropellerForwardAxis(false);
    }
    
    protected virtual void OnStartEngine()
    {
        engineOn = !engineOn;
        
        sfxManager.PlaySound(engineOn ? AudioManager.ESounds.StartEngine : AudioManager.ESounds.OffEngine);
        if (engineOn && mainPropellerSpinRate < 1) mainPropellerSpinRate += 1f;
    }

    protected virtual void Lift()
    {
        
    }
    
    protected virtual void YawControl()
    {
        
    }
    
    protected virtual void PitchControl()
    {
        
    }
    
    protected virtual void RollControl()
    {
        
    }

    // Sets the position and rotation whilst keeping the propellers on
    public virtual void Move(Vector3 pos, Quaternion rot)
    {
        engineOn = true;
        maxPropellerSpinRate = stats.maxMainPropellerAccelSpinRate;
        mainPropellerSpinRate = stats.maxMainPropellerAccelSpinRate;
        
        mainRb.Sleep();
        
        mainRb.transform.position = pos;
        mainRb.transform.rotation = rot;
        
        mainRb.angularVelocity = Vector3.zero;
        mainRb.linearVelocity = Vector3.zero;
        
        mainRb.WakeUp();
    }
    
    // Used to go back to the spawn point of the aviator
    public void Respawn(bool pause = true)
    {
        mainPropellerSpinRate = 0;
        mainPropellerRb.angularVelocity = Vector3.zero; 
        
        mainRb.Sleep();

        engineOn = false;
        
        // Go back to the parent's transform
        mainRb.transform.localPosition = Vector3.zero;
        mainRb.transform.localRotation = Quaternion.identity;
        
        // Remove velocity
        mainRb.angularVelocity = Vector3.zero;
        mainRb.linearVelocity = Vector3.zero;
        
        mainRb.WakeUp();
        
        // Since you can respawn from outside the pause menu
        if(pause) OnPause();
        
        // End the race manually
        scoreManager.EndRace(true);
    }

    public void SetActive(bool active) { gameObject.SetActive(active); }
    
    // Called by hoop manager whenever the player goes through a hoop
    public bool ThroughHoop()
    {
        sfxManager.PlaySound(AudioManager.ESounds.ThroughHoop);
        return scoreManager.ThroughHoop();
    }

    public void LockMouse(ushort num) { inputManager.LockMouse(0); }
    public void UnlockMouse() { inputManager.UnlockMouse(false); }
    
    
    
    //////// Getters
    protected Vector3 GetPropellerForwardAxis(bool flip = false)
    {
        Vector3 localForward;
        if (mainPropellerSpinAxis.x != 0) localForward = mainPropellerRb.transform.right;
        else if (mainPropellerSpinAxis.y != 0) localForward = mainPropellerRb.transform.up;
        else localForward = mainPropellerRb.transform.forward;

        return flip? -localForward : localForward;
    }

    public Vector3 GetForwardAxis(bool flip = false)
    {
        Vector3 direction;
        if (bodyForwardAxis.x != 0) direction = mainRb.transform.right;
        else if (bodyForwardAxis.y != 0) direction = mainRb.transform.up;
        else direction = mainRb.transform.forward;
        
        return flip? -direction : direction;
    }
    
    public Vector3 GetUpAxis(bool flip = false)
    {
        Vector3 direction;
        if (bodyUpAxis.x != 0) direction = mainRb.transform.right;
        else if (bodyUpAxis.y != 0) direction = mainRb.transform.up;
        else direction = mainRb.transform.forward;
        
        return flip? -direction : direction;
    }
    
    public ScoreManager GetScoreManager() { return scoreManager; }

    // Gets the transform of the actual moving part of the prefab (the parent doesn't move). 
    public Vector3 GetPosition() { return mainRb.transform.position; }
    public Quaternion GetRotation() { return mainRb.transform.rotation; }


    
    //////// Button Presses
    public void OnPause() { onGamePaused?.Invoke(); }
    
    // Used to go to the last hoop the player went through
    void OnRespawn()
    {
        if(scoreManager.score > 0) onRetry?.Invoke(); 
        else Respawn(false);
    }
    
    void OnToggleCamera()
    {
        lookingAtHoop = !lookingAtHoop;

        CinemachineTargetGroup targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
        if (lookingAtHoop) targetGroup.Targets.Add(hoopGroupTarget);
        else targetGroup.Targets.Remove(hoopGroupTarget);
        
        cam.Target.TrackingTarget = mainRb.transform;
        
        // Don't lock look inputs if the group target doesn't contain the hoop 
        if (!hoopGroupTarget.Object.gameObject.activeSelf) return;
        
        CinemachineInputAxisController camInputs = cam.GetComponent<CinemachineInputAxisController>();
        camInputs.enabled = !lookingAtHoop;
    }
    
    private void OnFlip()
    {
        bool isMovingSlowly = mainRb.linearVelocity.magnitude < 1;
        bool isUpsideDown = Vector3.Dot(mainRb.transform.up, Vector3.down) > 0.65f;
        // print($"dot: {Vector3.Dot(mainRb.transform.up, Vector3.down)}");

        if (!isMovingSlowly || !isUpsideDown) return;
        
        mainRb.AddForce(Vector3.up * mainRb.mass * 250);
        mainRb.AddRelativeTorque(0,0,mainRb.mass * 100000);
    }
    
    private void OnChangeAviator()
    {
        if(Time.timeScale != 0) OnPause();
        uiManager.ShowAviatorSelect();
    }
    
    private void OnFiveHoops() { onRaceStart?.Invoke(5); }
    private void OnTenHoops() { onRaceStart?.Invoke(10); }
    private void OnTwentyHoops() { onRaceStart?.Invoke(20); }
    private void OnUnlimitedHoops() { onRaceStart?.Invoke(0); }
}