using System;
using UnityEngine;
using UnityEngine.UI;

public class HoopScript : MonoBehaviour
{
    [SerializeField] 
    private Color colour = Color.cyan;
    
    public HoopScript(Transform spawnTransform)
    {
        transform.position = spawnTransform.position;
        transform.rotation = spawnTransform.rotation;
    }
    
    public event OnCollided onCollision;
    public delegate void OnCollided();

    private void OnTriggerEnter(Collider other)
    {
        print("start");
        // Don't do anything unless an aviator collides with it.
        if(!other.gameObject.GetComponentInParent<AviatorController>()) return;
        
        print("triggered");
        onCollision?.Invoke();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Image img = GetComponentInChildren<Image>();
        img.color = colour;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reposition(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}
