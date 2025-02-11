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

    // How many hoops the player has gone through
    private int collidedHoops;
    
    [Space]
    [SerializeField, Tooltip("The max spawn distance for new hoops.")]
    private float maxSpawnDistance = 600;
    [SerializeField, Tooltip("The min spawn distance for new hoops.")]
    private float minSpawnDistance = 100;

    [SerializeField] 
    private float maxAngle = 60;

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
        player.onRespawn += Respawn;
        
        currentHoop.onCollision += NewHoop;
        Init();
    }

    void Init()
    {
        Vector3 pos = new();
        
        RandomPos(ref pos);
        currentHoop.Reposition(pos);
        hoopVfx.transform.position = currentHoop.transform.position;
    }

    void RandomPos(ref Vector3 randPos)
    {
        // Get a random angle in range
        float randomYaw = Random.Range(-maxAngle, maxAngle);
        float randomPitch = Random.Range(-maxAngle, maxAngle);
        Quaternion rotationOffset = Quaternion.Euler(randomPitch, randomYaw, 0);

        // Apply the rotation to get a direction
        Vector3 randDirection = rotationOffset * -nextHoop.transform.forward;
        float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        randPos = nextHoop.transform.position + randDirection * spawnDistance;
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
        
        player.ThroughHoop();
        collidedHoops++;
    }

    public HoopScript GetCurrentHoop() { return currentHoop; }
    
    void Respawn()
    {
        if(collidedHoops == 0) return;
        
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
}
