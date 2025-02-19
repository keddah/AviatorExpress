using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private AviatorController player;

    [SerializeField]
    private AviatorSelect selector;

    public void ToMainMenu()
    {
        Time.timeScale = 1;

        foreach (var selector in FindObjectsByType<AviatorSelect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            SceneManager.sceneLoaded -= selector.ChangeAviator;
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadSceneAsync("Lvl_MainMenu");
    }

    public void AviatorSelect()
    {
        selector.ShowHide(true);
    }

    public void ShowHide(bool show)
    {
        // Pause the game if show
        Time.timeScale = show ? 0 : 1;
        
        if(show) player.UnlockMouse();
        else player.LockMouse(0);
        
        gameObject.SetActive(show);
    }

    public void Respawn()
    {
        player.Respawn();
    }
}
