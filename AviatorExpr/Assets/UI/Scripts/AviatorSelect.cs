using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AviatorSelect : MonoBehaviour
{
    private AviatorController player;
    private AviatorStats.EAviatorType selectedAviator;
    
    private void Awake()
    {
        selectedAviator = AviatorStats.EAviatorType.None;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<AviatorController>();
        SceneManager.sceneLoaded += ChangeAviator;
    }

    public void ShowHide(bool show) { gameObject.SetActive(show); }
    
    public void Play(string type)
    {
        selectedAviator = type.ToUpper() == "PLANE"? AviatorStats.EAviatorType.Plane : AviatorStats.EAviatorType.Helicopter;
        
        // Select the aviator.. the process should be different if in-game
        if (SceneManager.GetActiveScene().name == "Lvl_MainMenu")
        {
            
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.LoadScene("Scenes/main");
            return;
        }

        if(!player) player = GetComponentInParent<AviatorController>();
        ChangeAviator(SceneManager.GetActiveScene());
        ShowHide(false);
        player.OnPause();
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
                Helicopter heli = FindAnyObjectByType<Helicopter>();
                if (!heli) break;
                
                heli.SetActive(true);
                if(player) heli.Move(player.GetPosition(), player.GetRotation());
                // heli.Respawn();
                break;
                
            case AviatorStats.EAviatorType.Plane:
                Plane plane = FindAnyObjectByType<Plane>();
                if (!plane) break;
                
                plane.SetActive(true);
                if(player) plane.Move(player.GetPosition(), player.GetRotation());
                // plane.Respawn();
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(selectedAviator), selectedAviator, null);
        }
    }
}
