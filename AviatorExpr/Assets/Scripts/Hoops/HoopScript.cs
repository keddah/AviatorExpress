using UnityEngine;
using UnityEngine.UI;

public class HoopScript : MonoBehaviour
{
    private bool collided = false;
    
    public HoopScript(Transform spawnTransform)
    {
        transform.position = spawnTransform.position;
        transform.rotation = spawnTransform.rotation;
    }
    
    public event OnCollided onCollision;
    public delegate void OnCollided();

    private void OnTriggerEnter(Collider other)
    {
        // To prevent it from being called from the same collision
        if(collided) return;
        
        // Don't do anything unless an aviator collides with it.
        if(!other.gameObject.GetComponentInParent<AviatorController>()) return;
        
        print(other.gameObject.name);

        collided = true;
        onCollision?.Invoke();
    }

    public void Reposition(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        collided = false;
    }
}
