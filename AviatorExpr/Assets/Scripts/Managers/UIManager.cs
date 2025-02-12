using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI timeTxt;
    
    [SerializeField] 
    private TextMeshProUGUI scoreTxt;

    [SerializeField] 
    private TextMeshProUGUI popupScoreTxt;
    [SerializeField] 
    private float popupFadeSpeed = .7f;

    private AviatorController player;
    private ScoreManager scoreManager;

    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
        scoreManager = player.GetScoreManager();
        scoreManager.onScoreAdded += UpdateScore;
        popupScoreTxt.alpha = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float time = scoreManager.time;
        int totalSeconds = Mathf.FloorToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
    
        timeTxt.text = $"Time: {minutes:D2}:{seconds:D2}";
        popupScoreTxt.alpha -= Time.deltaTime * popupFadeSpeed;
    }

    void UpdateScore(int newScore, int addedScore)
    {
        scoreTxt.text = $"Score: {newScore}";
        popupScoreTxt.text = $"+{addedScore}";

        Rigidbody playerRb = player.GetComponentInChildren<Rigidbody>();

        // Show the popup
        popupScoreTxt.alpha = 1;

        Vector3 offset = playerRb.linearVelocity * 1.2f + player.GetUpAxis() * 10;
        popupScoreTxt.transform.position = playerRb.transform.position + offset;
        popupScoreTxt.transform.rotation = Quaternion.LookRotation(player.GetForwardAxis());
    }
}
