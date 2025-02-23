/**************************************************************************************************************
* Aviator Selector  
*
*   All the available aviators should be in the main scene. Before the player goes into the main scene, the player has to choose which aviator they want  
*   to use. When in the main menu, once an aviator has been selected, this loads the main scene and activates the corresponding aviator.
*   When in-game, when a new aviator is chosen, the new aviator seamlessly transitions to wherever the previous one was.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AviatorSelect : MonoBehaviour
{
    public event OnChangedAviator onAviatorChange;
    public delegate void OnChangedAviator(AviatorController newPlayer);
    
    private AviatorController player;
    private Rigidbody playerRb;
    private AviatorStats.EAviatorType selectedAviator;
    
    private void Awake()
    {
        selectedAviator = AviatorStats.EAviatorType.None;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<AviatorController>(true);
        
        SceneManager.sceneLoaded += ChangeAviator;
    }

    public void ShowHide(bool show) { gameObject.SetActive(show); }
    
    public void Play(string type)
    {
        selectedAviator = type.ToUpper() == "PLANE"? AviatorStats.EAviatorType.Plane : AviatorStats.EAviatorType.Helicopter;
        
        // Select the aviator.. the process should be different if in-game
        if (SceneManager.GetActiveScene().name == "Lvl_MainMenu")
        {
            // Load into the main scene. Change aviator will be called once the seen is loaded (onSceneLoaded)
            SceneManager.LoadSceneAsync("Scenes/main");
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            return;
        }

        player.OnPause();
        ChangeAviator(SceneManager.GetActiveScene());
        ShowHide(false);
    }

    public void ChangeAviator(Scene scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (scene.name == "Lvl_MainMenu")
        {
            print("returned because main menu");
            return;
        }
        
        // Deactivate 'this'
        if (player) player.SetActive(false);
        else print("player not found");

        // Each Aviator type should be active in the level to start with
        switch (selectedAviator)
        {
            // Deactivating all the aviator prefabs that isn't the chosen one.
            case AviatorStats.EAviatorType.Helicopter:
                Helicopter heli = FindAnyObjectByType<Helicopter>(FindObjectsInactive.Include);
                if (!heli) break;
                
                if(player) heli.Move(player.GetPosition(), player.GetRotation());
                heli.SetActive(true);
                onAviatorChange?.Invoke(heli);
                break;
                
            case AviatorStats.EAviatorType.Plane:
                Plane plane = FindAnyObjectByType<Plane>(FindObjectsInactive.Include);
                if (!plane) break;
                
                if(player) plane.Move(player.GetPosition(), player.GetRotation());
                plane.SetActive(true);
                onAviatorChange?.Invoke(plane);
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(selectedAviator), selectedAviator, null);
        }
    }
}
