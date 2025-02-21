using System;
using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private AviatorSelect selector;

    private AudioSource music;
    
    [SerializeField]
    private AudioClip[] songs;

    private byte songIndex;
    
    private void Awake()
    {
        selector = GetComponentInChildren<AviatorSelect>();
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
