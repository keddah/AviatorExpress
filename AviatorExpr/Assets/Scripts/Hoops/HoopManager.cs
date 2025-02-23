/**************************************************************************************************************
* Hoop Manager  
*
*   Keeps track of the hoops that the player flies through. 
*   It moves the hoops randomly. Where they end up is dependent on the position of the hoop it's paired with.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class HoopManager : MonoBehaviour
{
    private AviatorController player;
    private Rigidbody activePlayerBody;
    
    private VisualEffect hoopVfx;
    
    [SerializeField]
    private HoopScript currentHoop;
    [SerializeField]
    private HoopScript nextHoop;

    private Vector3 previousHoopPos;
    private Quaternion previousHoopRot;

    [Header("Move Properties")]
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
        
        currentHoop.ShowHide(false);
        nextHoop.ShowHide(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var selector in FindObjectsByType<AviatorSelect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            selector.onAviatorChange += NewPlayer;
            currentHoop.BindDelegates(selector);
            nextHoop.BindDelegates(selector);
        }

        NewPlayer(FindAnyObjectByType<AviatorController>(FindObjectsInactive.Exclude));
        currentHoop.onCollision += NewHoop;
    }

    void NewPlayer(AviatorController newPlayer)
    {
        print("new player called ~ hoop manager");

        if (player)
        {
            player.onRetry -= Retry;
            player.onRaceStart -= Init;
            player.GetScoreManager().onEndRace -= EndRace;
        }
        
        player = newPlayer;
        activePlayerBody = player.GetComponentInChildren<Rigidbody>();
        
        // Reassign delegates if they haven't been assigned
        player.onRetry += Retry;
        player.onRaceStart += Init;
        player.GetScoreManager().onEndRace += EndRace;
    }

    void Init(ushort numHoops)
    {
        ShowHoops();
        
        // Move the first hoop to where the parent is
        currentHoop.Reposition(transform.position);
        
        // move the inactive hoop directly behind the first hoop to get the correct direction.
        nextHoop.transform.localPosition = new(0, 0, -35);
        
        Vector3 pos = new();
        RandomPos(ref pos);
        nextHoop.transform.position = pos;
        hoopVfx.transform.position = currentHoop.transform.position;
    }

    void RandomPos(ref Vector3 randPos)
    {
        var maxAttempts = 10;
        bool positionValid = false;

        for (var attempts = 0; attempts < maxAttempts; attempts++)
        {
            // Get a random angle in range
            float randomYaw = Random.Range(-maxYawAngle, maxYawAngle);
            float randomPitch = Random.Range(-maxPitchAngle, maxPitchAngle);
            Quaternion rotationOffset = Quaternion.Euler(randomPitch, randomYaw, 0);

            Vector3 randDirection = rotationOffset * -nextHoop.transform.forward;
            float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 potentialPos = nextHoop.transform.position + randDirection * spawnDistance;

            // Raycast from nextHoop towards the potential position
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
        hoopVfx.transform.position = activePlayerBody.transform.position + activePlayerBody.linearVelocity * 1.5f;
        hoopVfx.Play();

        previousHoopPos = currentHoop.transform.position;
        previousHoopRot = activePlayerBody.transform.rotation;
        
        // Move the current hoop to the next hoop 
        currentHoop.Reposition(nextHoop.transform.position);

        // A random location within a certain angle from the player.
        Vector3 pos = new();
        RandomPos(ref pos);
        
        // Move the next hoop to a new location 
        nextHoop.transform.position = pos;

        // Hide the next hoop if it's the penultimate hoop
        nextHoop.ShowHide(!player.ThroughHoop());
    }

    public HoopScript GetCurrentHoop() { return currentHoop; }
    
    void Retry()
    {
        activePlayerBody.Sleep();
        activePlayerBody.linearVelocity = Vector3.zero;
        activePlayerBody.angularVelocity = Vector3.zero;

        activePlayerBody.Move(previousHoopPos, previousHoopRot);

        activePlayerBody.WakeUp();
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
        
        activePlayerBody.linearVelocity = Vector3.zero;
        activePlayerBody.angularVelocity = Vector3.zero;
    }

    void EndRace(bool manually) { HideHoops(); }
    void HideHoops() { nextHoop.ShowHide(false); currentHoop.ShowHide(false); }
    void ShowHoops() { currentHoop.ShowHide(true); nextHoop.ShowHide(true); }
}
