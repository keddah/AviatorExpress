using UnityEngine;

public class HoopScript : MonoBehaviour
{
    // Using rigidbody to ensure it's the part of the player prefab that's moving
    private GameObject player;
    [SerializeField] private float lookSpeed = 1;
    
    public event OnCollided onCollision;
    public delegate void OnCollided();

    private void Start()
    {
        foreach (var selector in FindObjectsByType<AviatorSelect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            selector.onAviatorChange += NewPlayer;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // To prevent it from being called from the same collision
        if(other.gameObject != player) return;
        print(other.gameObject.name);
        onCollision?.Invoke();
    }

    void NewPlayer(AviatorController newPlayer)
    {
        if(!newPlayer) return;
        
        player = newPlayer.GetComponentInChildren<Rigidbody>().gameObject;
    }
    
    private void FixedUpdate()
    {
        if (!player)
        {
            print("no player ~ hoop script");
            return;
        }
        
        Quaternion lookRot = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }

    public void Reposition(Vector3 pos) { transform.position = pos; }

    public void ShowHide(bool show) { gameObject.SetActive(show); }
}
