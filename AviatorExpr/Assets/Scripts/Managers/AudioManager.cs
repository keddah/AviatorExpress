using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum ESounds
    {
        StartEngine,
        OffEngine,
        Engine,
        Propeller,
    }
    
    [SerializeField] 
    private Rigidbody propellerRb;

    [Header("Sound Players")]
    [SerializeField] 
    private AudioSource enginePlayer;
    
    [SerializeField] 
    private AudioSource propellerPlayer;
    
    private AudioSource uiPlayer;


    private bool engineOn;

    private void Awake()
    {
        uiPlayer = gameObject.AddComponent<AudioSource>();
        uiPlayer.spatialize = false;

        propellerPlayer.playOnAwake = false;
        propellerPlayer.loop = true;
        propellerPlayer.dopplerLevel = 4;
        propellerPlayer.spread = 60;
        propellerPlayer.spatialBlend = .875f;
        propellerPlayer.spatialize = true;

        enginePlayer.playOnAwake = false;
        enginePlayer.loop = true;
        enginePlayer.dopplerLevel = 5;
        enginePlayer.spread = 120;
        enginePlayer.spatialize = true;
        enginePlayer.spatialBlend = 1;
    }

    private void Start()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        if (!engineOn) return;
        
        UpdatePropeller();
        UpdateEngine();
    }

    public void PlaySound(ESounds toPlay)
    {
        switch (toPlay)
        {
            case ESounds.StartEngine:
                PlayStartEngine();
                break;
            
            case ESounds.OffEngine:
                PlayOffEngine();
                break;
            
            case ESounds.Engine:
                UpdateEngine();
                break;
            
            case ESounds.Propeller:
                UpdatePropeller();
                break;
        }
    }

    void UpdatePropeller()
    {
        if(!engineOn) return;

        float bladeSpeed = propellerRb.angularVelocity.magnitude;
        propellerPlayer.pitch = Mathf.Lerp(.95f, 2.5f, bladeSpeed / propellerRb.maxAngularVelocity);
        float bladePassingFreq = (bladeSpeed / 60) * 3;
        propellerPlayer.pitch *= 1 + bladePassingFreq / 100;
    }
    
    void UpdateEngine()
    {
    }
    
    private void PlayStartEngine()
    {
        engineOn = true;
        StartCoroutine(FadeInSound(ESounds.Propeller, 1.5f));
        StartCoroutine(FadeInSound(ESounds.Engine, .75f));
    }

    IEnumerator FadeInSound(ESounds sound, float fadeDuration)
    {
        float startVolume = 0;
        float targetVolume = 1;
        float timer = 0;

        AudioSource player;
        switch (sound)
        {
            case ESounds.Propeller:
                player = propellerPlayer;
                break;
            
            case ESounds.Engine:
                player = enginePlayer;
                break;
            default:
                player = new AudioSource();
                break;
        }
        
        player.volume = startVolume;
        player.Play();

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            player.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        player.volume = targetVolume;
    }
    
    private void PlayOffEngine()
    {
        engineOn = false;
    }
    
}
