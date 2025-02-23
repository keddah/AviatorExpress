/**************************************************************************************************************
* UI Manager  
*
*   The script for the HUD. Displays the time, the score, the hoops counter (for trials).
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] 
    private TextMeshProUGUI timeTxt;
    
    [SerializeField] 
    private TextMeshProUGUI scoreTxt;

    [SerializeField] 
    private TextMeshProUGUI hoopTxt;
    
    [SerializeField] 
    private TextMeshProUGUI popupScoreTxt;
    
    [SerializeField] 
    private TextMeshProUGUI respawnTxt;
    
    
    [Header("Backgrounds")]
    [SerializeField] 
    private GameObject timeBkg;
    
    [SerializeField] 
    private GameObject scoreParent;
    
    [SerializeField] 
    private RectTransform hoopBkg;


    [Header("Other UI")]
    [SerializeField]
    private RankBoard rankBoard;
    [SerializeField]
    private PauseMenu pauseMenu;

    [Space]
    [SerializeField] 
    private float popupFadeSpeed = .7f;

    
    private AviatorController player;
    private ScoreManager scoreManager;

    private Slider respawnSlider;
    
    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
        respawnSlider = respawnTxt.GetComponentInChildren<Slider>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreManager = player.GetScoreManager();
        scoreManager.onScoreAdded += UpdateScore;
        scoreManager.onEndRace += EndRace;

        player.onRaceStart += ShowHoops;
        player.onGamePaused += Pause;
        
        popupScoreTxt.alpha = 0;
        
        // Hide time and score
        scoreParent.SetActive(false);
        timeBkg.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var totalSeconds = Mathf.FloorToInt(scoreManager.time);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
    
        timeTxt.text = $"{minutes:D2}:{seconds:D2}";
        popupScoreTxt.alpha -= Time.deltaTime * popupFadeSpeed;
    }

    void UpdateScore(uint newScore, uint addedScore)
    {
        hoopTxt.text = $"{scoreManager.throughHoops} / {scoreManager.hoopTarget}";
        scoreTxt.text = newScore.ToString();
        popupScoreTxt.text = $"+{addedScore}";

        Rigidbody playerRb = player.GetComponentInChildren<Rigidbody>();

        // Show the popup
        popupScoreTxt.alpha = 1;

        Vector3 offset = playerRb.linearVelocity * 1.2f + player.GetUpAxis() * 10;
        popupScoreTxt.transform.position = playerRb.transform.position + offset;
        popupScoreTxt.transform.rotation = Quaternion.LookRotation(player.GetForwardAxis());
    }

    private void ShowHoops(byte numHoops)
    {
        scoreManager.Reset();
        scoreTxt.text = "0"; 
        
        // Show time and score
        scoreParent.SetActive(true);
        timeBkg.SetActive(true);
        
        scoreManager.SetHoopTarget(numHoops);
        hoopBkg.gameObject.SetActive(true);
        hoopTxt.text = $"0/{scoreManager.hoopTarget}";
        
        if(numHoops == 0) HideHoops();
    }

    private void HideHoops()
    {
        hoopBkg.gameObject.SetActive(false);
    }

    private void EndRace(bool manually)
    {
        // Hide score
        HideHoops();
        scoreParent.SetActive(false);
        
        // Manually means the player forced the race to end by respawning 
        if (manually)
        {
            timeBkg.SetActive(false);
            return;
        }
        
        rankBoard.ShowBoard(scoreManager.timeStamps, scoreManager.score);
    }
    
    public void HideRankBoard()
    {
        rankBoard.HideBoard();
        timeBkg.SetActive(false);
        player.LockMouse(0);
    }
    
    void Pause()
    {
        bool isPaused = Time.timeScale == 0;
        pauseMenu.ShowHide(!isPaused);
    }

    public void ShowAviatorSelect() { pauseMenu.AviatorSelect(); }

    // Called from the player's Update()
    public void ShowHideRespawning(float currentHoldTime, float buttonHoldTime)
    {
        if (currentHoldTime <= 0)
        {
            respawnTxt.gameObject.SetActive(false);
            return;
        }

        // Unhide the text 
        respawnTxt.gameObject.SetActive(true);
        
        // Set the slider percentage
        respawnSlider.value = currentHoldTime / buttonHoldTime;
        // print($"current time: {currentHoldTime}");
        // print($"hold: {buttonHoldTime}");
        // print($"value: {respawnSlider.value}");
    }
}
