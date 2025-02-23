/**************************************************************************************************************
* Main Menu 
*
*   Provides a quit game function and allows for music to loop in a playlist. 
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private AudioSource music;
    
    [SerializeField]
    private AudioClip[] songs;

    private byte songIndex;
    
    private void Awake()
    {
        music = FindAnyObjectByType<AudioSource>();
        Loop();
    }

    void Loop()
    {
        music.clip = songs[songIndex];
        music.loop = false;
        
        music.Play();
        StartCoroutine(Delay(music.clip.length, Loop));

        songIndex++;
        if (songIndex >= songs.Length) songIndex = 0;
    }
    
    IEnumerator Delay(float delay, Action function)
    {
        yield return new WaitForSeconds(delay);
        function?.Invoke();
    }

    public void CloseGame() { Application.Quit(); }
}
