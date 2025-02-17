using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Texts")]
    
    [SerializeField] 
    private TextMeshProUGUI timeTxt;
    
    [SerializeField] 
    private GameObject timeBkg;
    
    [SerializeField] 
    private GameObject scoreParent;
    
    [SerializeField] 
    private TextMeshProUGUI scoreTxt;

    [SerializeField] 
    private TextMeshProUGUI hoopTxt;
    
    [SerializeField] 
    private RectTransform hoopBkg;

    [SerializeField] 
    private TextMeshProUGUI popupScoreTxt;
    [SerializeField] 
    private float popupFadeSpeed = .7f;

    private AviatorController player;
    private ScoreManager scoreManager;

    [Space]
    [SerializeField] 
    private RectTransform rankBoardBkg;

    private RankBoard rankBoard;
    
    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
        rankBoard = rankBoardBkg.GetComponentInParent<RankBoard>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        scoreManager = player.GetScoreManager();
        scoreManager.onScoreAdded += UpdateScore;
        scoreManager.onEndRace += EndRace;

        player.onRaceStart += ShowHoops;
        
        popupScoreTxt.alpha = 0;
        
        // Hide time and score
        scoreParent.SetActive(false);
        timeBkg.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float time = scoreManager.time;
        int totalSeconds = Mathf.FloorToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
    
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

    private void ShowHoops(ushort numHoops)
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

    private void EndRace()
    {
        // Hide score
        scoreParent.SetActive(false);
        rankBoard.ShowBoard(scoreManager.timeStamps, scoreManager.score);
        HideHoops();
    }
    

    public void HideRankBoard()
    {
        rankBoard.HideBoard();
        timeBkg.SetActive(false);
        player.LockMouse(0);
    }
}
