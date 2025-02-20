using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public Rigidbody mainRb { get; private set; }
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
    
    // The current spin rate
    protected float mainPropellerSpinRate;
    protected float maxPropellerSpinRate;

    protected float mainPropellerSpeed;

    
    ///////////// Controls
    protected InputController inputManager;
    private bool respawning;
    
    
    ///////////// Audio
    private AudioManager sfxManager;
    
    
    ///////////// Camera
    private bool lookingAtHoop;
    private CinemachineCamera cam;
    CinemachineTargetGroup.Target hoopGroupTarget;


    public bool engineOn { get; private set; }
    protected float altitude;

    [SerializeField] 
    private UIManager uiManager;

    [Space] 
    [SerializeField] 
    private ushort scoreTimeBonus = 25;
    private ScoreManager scoreManager;

    protected virtual void Awake()
    {
        scoreManager = new(scoreTimeBonus);
        
        inputManager = GetComponent<InputController>();
        sfxManager = GetComponent<AudioManager>();
    
        mainRb = mainObject.GetComponent<Rigidbody>();
        mainRb.interpolation = RigidbodyInterpolation.Interpolate;
        mainPropellerRb = mainPropeller.GetComponent<Rigidbody>();

        mainRb.centerOfMass = centerMass.transform.localPosition;

        HingeJoint propellerHinge = mainPropeller.GetComponent<HingeJoint>();
        propellerHinge.axis = mainPropellerSpinAxis;

        cam = GetComponentInChildren<CinemachineCamera>();
        
        SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        scoreManager.onEndRace += inputManager.UnlockMouse;
        onRaceStart += LockMouse;
        
        mainPropellerRb.maxAngularVelocity = stats.maxMainPropellerAccelSpinRate + 5;

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
        IsRespawning();
        scoreManager.Update();
        
        mainPropellerSpeed = mainPropellerRb.angularVelocity.magnitude;
        altitude = mainRb.transform.position.y;
        
        ThrottleControl();
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
        if (inputManager.throttleUpValue > 0) maxPropellerSpinRate = stats.maxMainPropellerAccelSpinRate * inputManager.throttleUpValue;
        else if (inputManager.throttleDownValue > 0) maxPropellerSpinRate = stats.maxMainPropellerDecelSpinRate;
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
    
    protected Vector3 GetPropellerForwardAxis(bool flip = false)
    {
        Vector3 localForward;
        if (mainPropellerSpinAxis.x != 0) localForward = mainPropellerRb.transform.right;
        else if (mainPropellerSpinAxis.y != 0) localForward = mainPropellerRb.transform.up;
        else localForward = mainPropellerRb.transform.forward;

        return flip? -localForward : localForward;
    }

    public Vector3 GetWorldUpAxis() { return bodyUpAxis; }
    
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

    private void IsRespawning()
    {
        respawning = inputManager.respawnPressed;
        if(respawning) print("Respawning...");
    }
    
    // Used to go to the last hoop the player went through
    void OnRespawn() { onRetry?.Invoke(); }

    // Sets the position and rotation while keeping the propeller's state
    virtual public void Move(Vector3 pos, Quaternion rot)
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
    public void Respawn()
    {
        mainPropellerSpinRate = 0;
        mainPropellerRb.angularVelocity = Vector3.zero; 
        
        mainRb.Sleep();

        engineOn = false;
        
        mainRb.transform.localPosition = Vector3.zero;
        mainRb.transform.localRotation = Quaternion.identity;
        
        mainRb.angularVelocity = Vector3.zero;
        mainRb.linearVelocity = Vector3.zero;
        
        mainRb.WakeUp();
        
        OnPause();
    }

    public Vector3 GetPosition() { return mainRb.transform.position; }
    public Quaternion GetRotation() { return mainRb.transform.rotation; }
    
    public void SetActive(bool active)
    {
        mainObject.SetActive(active);

        sfxManager.enabled = active;
        
        GetComponent<PlayerInput>().enabled = active;
        inputManager.enabled = active;

        uiManager.gameObject.SetActive(active);
        enabled = active;
    }
    
    
    
    public void OnPause() { onGamePaused?.Invoke(); }
    
    void OnToggleCamera()
    {
        lookingAtHoop = !lookingAtHoop;

        CinemachineTargetGroup targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
        if (lookingAtHoop) targetGroup.Targets.Add(hoopGroupTarget);
        else targetGroup.Targets.Remove(hoopGroupTarget);
        
        cam.Target.TrackingTarget = mainRb.transform;
        CinemachineInputAxisController camInputs = cam.GetComponent<CinemachineInputAxisController>();
        camInputs.enabled = !lookingAtHoop;
    }

    // Called by hoop manager whenever the player goes through a hoop
    public bool ThroughHoop()
    {
        sfxManager.PlaySound(AudioManager.ESounds.ThroughHoop);
        return scoreManager.ThroughHoop();
    }

    public ScoreManager GetScoreManager() { return scoreManager; }

    public void LockMouse(ushort num) { inputManager.LockMouse(0); }
    public void UnlockMouse() { inputManager.UnlockMouse(); }

    void OnFlip()
    {
        bool canFlip = mainRb.linearVelocity.magnitude < 1;
        if(!canFlip) return;

        // Is the plane upside down
        canFlip = Math.Abs(Vector3.Dot(mainRb.transform.up, Vector3.down)) > .65f;
        print(Math.Abs(Vector3.Dot(mainRb.transform.up, Vector3.down)));
        if(!canFlip) return;
        
        mainRb.AddForce(Vector3.up * mainRb.mass * 500);
        mainRb.AddRelativeTorque(0,0,mainRb.mass * 75000);
    }

    void OnChangeAviator()
    {
        if(Time.timeScale != 0) OnPause();
        uiManager.ShowAviatorSelect();
    }
    
    void OnFiveHoops() { onRaceStart?.Invoke(5); }
    void OnTenHoops() { onRaceStart?.Invoke(10); }
    void OnTwentyHoops() { onRaceStart?.Invoke(20); }
    void OnUnlimitedHoops() { onRaceStart?.Invoke(0); }
}