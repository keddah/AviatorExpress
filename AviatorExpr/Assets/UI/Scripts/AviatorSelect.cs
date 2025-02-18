using UnityEngine;
using UnityEngine.SceneManagement;

public class AviatorSelect : MonoBehaviour
{
    private AviatorController player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ShowHide(bool show) { gameObject.SetActive(show); }
    
    public void Play()
    {
        // Select the aviator.. the process should be different if in-game
        if (SceneManager.GetActiveScene().name == "Lvl_MainMenu")
        {
            SceneManager.LoadScene(SceneManager.sceneCount - 1);
            return;
        }

        if(!player) player = GetComponentInParent<AviatorController>();
        player.Respawn();
    }
}
