/**************************************************************************************************************
* Speed-o-Meter
*
*   Gets the player's speed, converts it to miles per hour, then displays it.  
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [SerializeField]
    private Rigidbody playerBody;

    [SerializeField]
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = playerBody.linearVelocity.magnitude;
        
        // Convert to mph
        speed *= 2.23694f; 
        
        text.text = ((int)speed).ToString("D5");
    }
}
