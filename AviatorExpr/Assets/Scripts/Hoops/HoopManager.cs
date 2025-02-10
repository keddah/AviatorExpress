using UnityEngine;
using Random = UnityEngine.Random;

public class HoopManager : MonoBehaviour
{
    private AviatorController player;
    private Rigidbody playerBody;

    private int hoopsMade;
    
    [SerializeField]
    private HoopScript currentHoop;
    [SerializeField]
    private HoopScript nextHoop;

    [Space]
    [SerializeField, Tooltip("The max spawn distance for new hoops.")]
    private float maxSpawnDistance = 600;
    [SerializeField, Tooltip("The min spawn distance for new hoops.")]
    private float minSpawnDistance = 100;

    [SerializeField] 
    private float maxAngle = 60;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<AviatorController>();
        playerBody = player.mainRb;
        currentHoop.onCollision += NewHoop;
        
        Init();
    }

    void Init()
    {
        Vector3 pos = new();
        
        RandomPos(ref pos);
        currentHoop.Reposition(pos);
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
        print("new hoop");
        // Move the current hoop to the next hoop 
        currentHoop.Reposition(nextHoop.transform.position);

        // A random location within a certain angle from the player.
        Vector3 pos = new();
        RandomPos(ref pos);
        
        // Move the next hoop to a new location 
        nextHoop.transform.position = pos;
        
        player.ThroughHoop();
    }

    public HoopScript GetCurrentHoop() { return currentHoop; }
}
