using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class HoopManager : MonoBehaviour
{
    private AviatorController player;
    private Rigidbody playerBody;
    
    private VisualEffect hoopVfx;
    
    [SerializeField]
    private HoopScript currentHoop;
    [SerializeField]
    private HoopScript nextHoop;

    private Vector3 previousHoopPos;
    private Quaternion previousHoopRot;

    [Space]
    [SerializeField, Tooltip("The max spawn distance for new hoops.")]
    private float maxSpawnDistance = 600;
    [SerializeField, Tooltip("The min spawn distance for new hoops.")]
    private float minSpawnDistance = 100;

    [SerializeField] 
    private float maxYawAngle = 60;
    
    [SerializeField] 
    private float maxPitchAngle = 45;

    private void Awake()
    {
        hoopVfx = GetComponentInChildren<VisualEffect>();
        hoopVfx.Stop();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<AviatorController>();
        playerBody = player.GetComponentInChildren<Rigidbody>();
       
        // Delegates
        currentHoop.onCollision += NewHoop;
        player.onRespawn += Respawn;
        player.onRaceStart += Init;
        player.GetScoreManager().onEndRace += EndRace;
        
        currentHoop.Hide();
        nextHoop.Hide();
    }

    void Init(ushort numHoops)
    {
        ShowHoops();
        
        nextHoop.transform.localPosition = new(0, 0, -35);
        currentHoop.Reposition(transform.position);
        
        Vector3 pos = new();
        RandomPos(ref pos);
        nextHoop.transform.position = pos;
        
        hoopVfx.transform.position = currentHoop.transform.position;
    }

    void RandomPos(ref Vector3 randPos)
    {
        var maxAttempts = 10; // Prevents infinite loops
        bool positionValid = false;

        for (var attempts = 0; attempts < maxAttempts; attempts++)
        {
            // Get a random angle in range
            float randomYaw = Random.Range(-maxYawAngle, maxYawAngle);
            float randomPitch = Random.Range(-maxPitchAngle, maxPitchAngle);
            Quaternion rotationOffset = Quaternion.Euler(randomPitch, randomYaw, 0);

            // Apply the rotation to get a direction
            Vector3 randDirection = rotationOffset * -nextHoop.transform.forward;
            float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

            // Compute potential position
            Vector3 potentialPos = nextHoop.transform.position + randDirection * spawnDistance;

            // Perform a raycast from nextHoop towards the potential position
            if (Physics.Raycast(nextHoop.transform.position, randDirection, spawnDistance)) continue;
            
            randPos = potentialPos;
            positionValid = true;
            break; 
        }

        // Fallback if no valid position is found
        if (positionValid) return;
        
        randPos = nextHoop.transform.position + nextHoop.transform.forward * maxSpawnDistance;
        Debug.LogWarning("Failed to find a valid position, using max spawn distance.");
    }



    void NewHoop()
    {
        // Move the vfx to in front of player
        hoopVfx.transform.position = playerBody.transform.position + playerBody.linearVelocity * 1.5f;
        hoopVfx.Play();

        previousHoopPos = currentHoop.transform.position;
        previousHoopRot = playerBody.transform.rotation;
        
        // Move the current hoop to the next hoop 
        currentHoop.Reposition(nextHoop.transform.position);

        // A random location within a certain angle from the player.
        Vector3 pos = new();
        RandomPos(ref pos);
        
        // Move the next hoop to a new location 
        nextHoop.transform.position = pos;
        
        if(player.ThroughHoop()) nextHoop.Hide();
        else nextHoop.Show();
    }

    public HoopScript GetCurrentHoop() { return currentHoop; }
    
    void Respawn()
    {
        playerBody.Sleep();
        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;

        playerBody.Move(previousHoopPos, previousHoopRot);

        playerBody.WakeUp();
        StartCoroutine(RemoveVelocity());
    }

    IEnumerator RemoveVelocity(float delay = .1f)
    {
        float timer = 0;
        while (timer < delay)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;
    }

    void EndRace()
    {
        HideHoops();
    }
    
    public void HideHoops() { nextHoop.Hide(); currentHoop.Hide(); }
    void ShowHoops()
    { 
        currentHoop.Show();
        nextHoop.Show();
    }
}
