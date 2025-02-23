/**************************************************************************************************************
* Hoop  
*
*   Just rotates to face the player and invokes a delegate whenever the player collides with it.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEngine;

public class HoopScript : MonoBehaviour
{
    // Using rigid body game object to ensure it's the part of the player prefab that's moving
    private GameObject player;
    [SerializeField] private float lookSpeed = 1;
    
    public event OnCollided onCollision;
    public delegate void OnCollided();

    // Since the hoops are hidden when the game starts, this is run once they become active 
    private void Start()
    {
        foreach (var selector in FindObjectsByType<AviatorSelect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            BindDelegates(selector);
        }
    }

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
            // For the first spawn, The player isn't found... (should only need to be run once per hoop at the start of the game)
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
    private void BindDelegates(AviatorSelect selector)
    {
        print("binding ~ hoop script");
        selector.onAviatorChange += NewPlayer;
    }
}
