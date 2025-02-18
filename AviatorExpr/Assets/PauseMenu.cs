using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private AviatorController player;

    private void Awake()
    {
        player = GetComponentInParent<AviatorController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToMainMenu()
    {
        
    }

    public void ShowHide(bool show)
    {
        // Pause the game if show
        Time.timeScale = show ? 0 : 1;
        
        gameObject.SetActive(show);
    }

    public void Respawn()
    {
        player.Respawn();
    }
}
