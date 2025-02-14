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

    private void Start()
    {
        HideBoard();
    }

    public void ShowBoard(List<float> timeStamps, ushort timeBonus, uint scorePerHoop, uint score)
    {
        CalculateRank(timeStamps, timeBonus, scorePerHoop, score);
        for(ushort i = 0; i < timeStamps.Count; i++) AddTimeStamp(i, timeStamps[i]);
        
        gameObject.SetActive(true);
    }

    public void HideBoard()
    {
        gameObject.SetActive(false);
    }
    
    private void AddTimeStamp(ushort hoopIndex, float hoopTime)
    {
        var newStamp = new GameObject();
        RectTransform rect = newStamp.AddComponent<RectTransform>();
        newStamp.AddComponent<CanvasRenderer>();
        TextMeshProUGUI text = newStamp.AddComponent<TextMeshProUGUI>();
        
        newStamp.transform.parent = stampsBkg.transform;
        // rect.anchorMin = new(.5f, 1);
        // rect.anchorMax = new(.5f, 1);
        // rect.pivot = new (.5f, 1);

        // float spacing = 30;
        // rect.anchoredPosition = new(0, hoopIndex * -spacing);
        
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

    private void CalculateRank(List<float> timeStamps, ushort timeBonus, uint scorePerHoop, uint score)
    {
        float totalTime = 0;
        foreach (float time in timeStamps) totalTime += time;
        
        // maxPoints = basePoints * numHoops + totalTime / numHoops * 75% timeBonus 
        var maxPoints = (scorePerHoop * timeStamps.Count + totalTime / timeStamps.Count * .75f * timeBonus);
        var scorePercentage = score / maxPoints;

        string rank;
        if (scorePercentage >= sRankPercent) rank = "S";
        else if (scorePercentage >= aRankPercent && scorePercentage < sRankPercent) rank = "A";
        else if (scorePercentage >= bRankPercent && scorePercentage < aRankPercent) rank = "B";
        else if (scorePercentage >= cRankPercent && scorePercentage < bRankPercent) rank = "C";
        else if (scorePercentage >= dRankPercent && scorePercentage < cRankPercent) rank = "D";
        else rank = "F";
        
        rankTxt.text = rank;
        scoreTxt.text = score.ToString();
    }
}
