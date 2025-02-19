using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AviatorSelect : MonoBehaviour
{
    private AviatorController player;
    private AviatorStats.EAviatorType selectedAviator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.sceneLoaded += ChangeAviator;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ShowHide(bool show) { gameObject.SetActive(show); }
    
    public void Play(string type)
    {
        selectedAviator = type.ToUpper() == "PLANE"? AviatorStats.EAviatorType.Plane : AviatorStats.EAviatorType.Helicopter;
        
        // Select the aviator.. the process should be different if in-game
        if (SceneManager.GetActiveScene().name == "Lvl_MainMenu")
        {
            SceneManager.LoadSceneAsync("Scenes/main");
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            return;
        }

        if(!player) player = GetComponentInParent<AviatorController>();
        ChangeAviator(SceneManager.GetActiveScene());
        player.Respawn();
    }

    void ChangeAviator(Scene scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if(scene.name == "Lvl_MainMenu") return;

        print("changing aviator");
        Helicopter heli = FindAnyObjectByType<Helicopter>(); 
        Plane plane = FindAnyObjectByType<Plane>();
        
        // Each Aviator type should be active in the level to start with
        switch (selectedAviator)
        {
            // Deactivating all the aviator prefabs that isn't the chosen one.
            case AviatorStats.EAviatorType.Helicopter:
                if(heli) heli.SetActive(true);
                if(plane) plane.SetActive(false);
                break;
                
            case AviatorStats.EAviatorType.Plane:
                if(plane) plane.SetActive(true);
                if(heli) heli.SetActive(false);
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(selectedAviator), selectedAviator, null);
        }
    }
}
