using System;
using UnityEngine;

public class ScoreManager
{
    public ScoreManager() {}
    
    public int score { get; private set; }
    public float time { get; private set; }

    private bool timerOn;
    
    // The time since the last hoop
    private float sinceLastHoop;
    
    // Getting to the hoop before this time increases the score obtained.
    private float speedBonusTime = 25;

    // How much score the player should get per hoop
    private int scoreIncrements = 100;
    private float scoreMultiplier = 1;

    public void Update()
    {
        if (timerOn)
        {
            time += Time.deltaTime;
            sinceLastHoop += Time.deltaTime;
            CalculateScoreMultiplier();
        }
    }

    public void StartTimer() { timerOn = true; }
    public void StopTimer() { timerOn = false; }
    
    public void ThroughHoop()
    {
        // Turn on the timer when the player goes through the first hoop
        if(score == 0) StartTimer();
        
        score += (int)(scoreIncrements * scoreMultiplier);
        sinceLastHoop = 0;
        Debug.Log($"Score: {score}");
    }

    private void CalculateScoreMultiplier()
    {
        // Use the time sinceLastHoop in a percentage against another number to get the multiplier 
        float delta = speedBonusTime - sinceLastHoop;
        float percentage = (delta / speedBonusTime) + 1;
        scoreMultiplier = Math.Max(percentage, 1);
    }
}
