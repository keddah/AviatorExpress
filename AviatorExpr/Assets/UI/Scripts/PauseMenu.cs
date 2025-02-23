/**************************************************************************************************************
* Pause Menu  
*
*   Sets the timescale to 0 (pausing the game) whenever the menu is made visible. Provides functions for the buttons in the menu - showing and hiding menus.
*   Also allows for music and sfx to be toggles on/off.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private AviatorController player;

    [SerializeField]
    private AviatorSelect selector;

    [Space]
    [SerializeField]
    private AudioMixer audioMixer;

    private bool sfxMuted;
    private bool musicMuted;
    
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

    public void ToggleMuteMusic(RectTransform buttonIcon)
    {
        musicMuted = !musicMuted;
        float volume = musicMuted ? -80f : 0f;
        audioMixer.SetFloat("MusicVolume", volume);
        
        GameObject icon = buttonIcon.GetComponentInChildren<Image>().gameObject;
        icon.SetActive(musicMuted);
    }
    
    public void ToggleMuteSound(RectTransform buttonIcon)
    {
        sfxMuted = !sfxMuted;
        float volume = sfxMuted ? -80f : 0f;
        audioMixer.SetFloat("SFXVolume", volume);
        
        GameObject icon = buttonIcon.GetComponent<Image>().gameObject;
        icon.SetActive(sfxMuted);
    }
}
