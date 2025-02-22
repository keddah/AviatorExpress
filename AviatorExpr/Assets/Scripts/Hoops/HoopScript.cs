using UnityEngine;

public class HoopScript : MonoBehaviour
{
    // Using rigidbody to ensure it's the part of the player prefab that's moving
    private GameObject player;
    [SerializeField] private float lookSpeed = 1;
    
    public event OnCollided onCollision;
    public delegate void OnCollided();

    private void OnTriggerEnter(Collider other)
    {
        // To prevent it from being called from the same collision
        if(other.gameObject != player) return;
        onCollision?.Invoke();
    }

    void NewPlayer(AviatorController newPlayer)
    {
        print("new player called ~ hoop script");
        if(!newPlayer) return;
        
        player = newPlayer.GetComponentInChildren<Rigidbody>(true).gameObject;
    }
    
    private void FixedUpdate()
    {
        if (!player)
        {
            // For the first spawn, The player isn't found... (should only need to be run once per hoop)
            print("getting player ~ hoop script");
            player = FindAnyObjectByType<AviatorController>(FindObjectsInactive.Exclude).GetComponentInChildren<Rigidbody>(true).gameObject;
            if (!player)
            {
                print("no player ~ hoop script");
                return;
            }
        }
        
        Quaternion lookRot = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }

    public void Reposition(Vector3 pos) { transform.position = pos; }

    public void ShowHide(bool show) { gameObject.SetActive(show); }
    
    // Called by the hoop manager since it doesn't work in start??
    public void BindDelegates(AviatorSelect selector)
    {
        print("binding ~ hoop script");
        selector.onAviatorChange += NewPlayer;
    }
}
