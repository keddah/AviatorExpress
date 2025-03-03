/**************************************************************************************************************
* Rank Board  
*
*   Gives the player a ranking depending on how fast they completed the hoop trial and shows the split timings for each hoop.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankBoard : MonoBehaviour
{
    [SerializeField] 
    private RectTransform stampsBkg;

    [SerializeField]
    private TextMeshProUGUI rankTxt;
    
    [SerializeField]
    private TextMeshProUGUI scoreTxt;
    
    [Header("Font")]
    [SerializeField] 
    private TMP_FontAsset timeStampFont;

    [SerializeField] 
    private float timeStampFontSize = 24;
    
    [SerializeField] 
    Color fontColour = Color.white;

    [SerializeField] 
    private float charSpacing = 5;

    [Header("Ranking")] 
    [SerializeField]
    private ushort avgTimePerHoop = 15;
    
    [Space]
    [SerializeField] 
    private float sRankPercent = .85f;
    [SerializeField] 
    private float aRankPercent = .75f;
    [SerializeField] 
    private float bRankPercent = .6f;
    [SerializeField] 
    private float cRankPercent = .55f;
    [SerializeField] 
    private float dRankPercent = .5f;
    [SerializeField] 
    private float fRankPercent = .4f;

    public void ShowBoard(List<float> timeStamps, uint score)
    {
        CalculateRank(timeStamps, score);
        for(ushort i = 0; i < timeStamps.Count; i++) AddTimeStamp(i, timeStamps[i]);
        
        gameObject.SetActive(true);
    }

    public void HideBoard()
    {
        gameObject.SetActive(false);
        ClearTimeStamps();
    }
    
    // Create the splits for the times it took the player to reach each hoop
    private void AddTimeStamp(ushort hoopIndex, float hoopTime)
    {
        var newStamp = new GameObject();
        newStamp.AddComponent<RectTransform>();
        newStamp.AddComponent<CanvasRenderer>();
        TextMeshProUGUI text = newStamp.AddComponent<TextMeshProUGUI>();
        
        // Set parent
        newStamp.transform.parent = stampsBkg.transform;
        
        // Setup text
        text.font = timeStampFont;
        text.color = fontColour;
        text.characterSpacing = charSpacing;
        text.fontSize = timeStampFontSize;
        
        // Hoop index
        string hoop = $"Hoop {hoopIndex + 1}:    ";
        
        // Time
        var totalSeconds = Mathf.FloorToInt(hoopTime);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        text.text = hoop + $"{minutes:D2}:{seconds:D2}";
    }

    private void ClearTimeStamps()
    {
        for (var i = 0; i < stampsBkg.transform.childCount; i++) Destroy(stampsBkg.transform.GetChild(i).gameObject);
    }
    
    private void CalculateRank(List<float> timeStamps, uint score)
    {
        float totalTime = 0;
        foreach (var time in timeStamps) totalTime += time;
        
        // --timeStampsCount because the timer doesn't start until the first hoop is reached
        float expectedTime = (timeStamps.Count - 1) * avgTimePerHoop;
        float timePercent = expectedTime / Mathf.Max(totalTime, 1f);
        // print($"Average total time: {(timeStamps.Count - 1) * avgTimePerHoop}");
        // print($"Total time: {totalTime}");
        // print($"Time percent: {timePercent}");
        
        string rank;
        if (timePercent >= sRankPercent) rank = "S";
        else if (timePercent >= aRankPercent && timePercent < sRankPercent) rank = "A";
        else if (timePercent >= bRankPercent && timePercent < aRankPercent) rank = "B";
        else if (timePercent >= cRankPercent && timePercent < bRankPercent) rank = "C";
        else if (timePercent >= dRankPercent && timePercent < cRankPercent) rank = "D";
        else rank = "F";
        
        rankTxt.text = rank;
        scoreTxt.text = score.ToString();
    }
}
