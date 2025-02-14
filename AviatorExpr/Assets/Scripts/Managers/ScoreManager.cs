using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    public event OnScoreAdded onScoreAdded;
    public delegate void OnScoreAdded(uint newScore, uint addedScore);
    
    public event OnRaceEnded onEndRace;
    public delegate void OnRaceEnded();

    public ScoreManager(ushort _speedBonusTime) { speedBonusTime = _speedBonusTime; }
    
    public uint score { get; private set; }
    public float time { get; private set; }

    private bool timerOn;
    
    // The time since the last hoop
    private float sinceLastHoop;

    public List<float> timeStamps { get; private set; } = new();
    
    // Getting to the hoop before this time increases the score obtained.
    public ushort speedBonusTime { get; private set; }

    // How much score the player should get per hoop
    public uint scorePerHoop { get; private set; } = 100;
    private float scoreMultiplier = 1;

    // How many hoops the player has gone through
    public ushort throughHoops { get; private set; }
    
    // The number of hoops the player is trying to collect
    public ushort hoopTarget { get; private set; }
    
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
    
    // Returns whether it's the penultimate hoop
    public bool ThroughHoop()
    {
        // Turn on the timer when the player goes through the first hoop
        if(score == 0) StartTimer();
        
        score += (uint)(scorePerHoop * scoreMultiplier);
        timeStamps.Add(sinceLastHoop);
        sinceLastHoop = 0;
        throughHoops++;
        onScoreAdded?.Invoke(score, (uint)(scorePerHoop * scoreMultiplier));
        
        if(hoopTarget == throughHoops) EndRace();

        return throughHoops == hoopTarget - 1;
    }

    private void CalculateScoreMultiplier()
    {
        // Use the time sinceLastHoop in a percentage against another number to get the multiplier 
        float delta = speedBonusTime - sinceLastHoop;
        float percentage = (delta / speedBonusTime) + 1;
        scoreMultiplier = Math.Max(percentage, 1);
    }

    public void SetHoopTarget(ushort target) { hoopTarget = target; }

    void EndRace()
    {
        StopTimer();
        onEndRace?.Invoke();
    }

    public void Reset()
    {
        timerOn = false;
        time = 0;
        sinceLastHoop = 0;
        timeStamps.Clear();
        
        throughHoops = 0;
        score = 0;
    }
}
