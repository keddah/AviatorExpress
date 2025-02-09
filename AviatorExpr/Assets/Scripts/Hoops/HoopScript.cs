using System;
using UnityEngine;

public class HoopScript : MonoBehaviour
{
    private bool collided = false;
    
    // Using rigidbody to ensure it's the part of the player prefab that's moving
    private Rigidbody player;
    [SerializeField] private float lookSpeed = 1;
    
    public event OnCollided onCollision;
    public delegate void OnCollided();

    private void Start()
    {
        player = FindAnyObjectByType<AviatorController>().GetComponentInChildren<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // To prevent it from being called from the same collision
        if(collided) return;
        if(!player) return;
        
        print(other.gameObject.name);

        collided = true;
        onCollision?.Invoke();
    }

    private void FixedUpdate()
    {
        Quaternion lookRot = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }

    public void Reposition(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        collided = false;
    }
}
