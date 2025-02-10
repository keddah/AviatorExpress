using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI timeTxt;
    
    [SerializeField] 
    private TextMeshProUGUI scoreTxt;

    private AviatorController player;
    private ScoreManager scoreManager;

    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
        scoreManager = player.GetScoreManager();
        scoreManager.onScoreAdded += UpdateScore;
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
    }

    void UpdateScore(int newScore) { scoreTxt.text = $"Score: {newScore}"; }
}
