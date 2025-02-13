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
        player = FindAnyObjectByType<AviatorController>().GetComponentInChildren<Rigidbody>().gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        // To prevent it from being called from the same collision
        if(other.gameObject != player) return;
        print(other.gameObject.name);
        onCollision?.Invoke();
    }

    private void FixedUpdate()
    {
        Quaternion lookRot = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }

    public void Reposition(Vector3 pos) { transform.position = pos; }
}
