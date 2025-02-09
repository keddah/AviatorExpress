using UnityEngine;
using Random = UnityEngine.Random;

public class HoopManager : MonoBehaviour
{
    [SerializeField, Tooltip("Assigned in the level")]
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerBody = player.mainRb;
        currentHoop.onCollision += NewHoop;
        
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init()
    {
        Vector3 pos = new();
        Quaternion rot = new();
        
        RandomTransform(ref pos, ref rot);
        currentHoop.Reposition(pos, rot);
    }

    void RandomTransform(ref Vector3 randPos, ref Quaternion randRot)
    {
        Vector3 randDirection = Random.onUnitSphere;
        randPos = playerBody.transform.position + randDirection * Random.Range(minSpawnDistance, maxSpawnDistance);
        randRot = playerBody.transform.rotation;
    }
    
    void NewHoop()
    {
        print("new hoop");
        // Move the current hoop to the next hoop 
        currentHoop.transform.position = nextHoop.transform.position;
        currentHoop.transform.rotation = nextHoop.transform.rotation;

        // A random location within a certain angle from the player.
        Vector3 pos = new();
        Quaternion rot = new();
        RandomTransform(ref pos, ref rot);
        
        // Move the next hoop to a new location 
        nextHoop.Reposition(pos, rot);
    }

    public HoopScript GetCurrentHoop() { return currentHoop; }
}
