/**************************************************************************************************************
* Arrow Indicator 
*
*   Just sets the rotation of the arrow so that it faces the target hoop.  
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    private AviatorController player;
    private HoopScript targetHoop;

    [SerializeField] 
    private float lookSpeed = 3;
    
    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetHoop = FindAnyObjectByType<HoopManager>().GetCurrentHoop();
        player.onRaceStart += Show;
        player.GetScoreManager().onEndRace += Hide;
        
        Hide();
    }

    void FixedUpdate()
    {
        Quaternion lookRot = Quaternion.LookRotation(targetHoop.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }

    void Show(ushort num) { gameObject.SetActive(true); }
    void Hide() { gameObject.SetActive(false); }
}
