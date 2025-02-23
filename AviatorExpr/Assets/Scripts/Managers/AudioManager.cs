/**************************************************************************************************************
* Audio Manager
* 
*   Creates an enum for the different sounds that play during gameplay. Sounds are played using PlaySound() using enum as a parameter.
*
* Created by Dean Atkinson-Walker 2025
***************************************************************************************************************/

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
        ThroughHoop
    }
    
    [Header("Rigid Bodies")] 
    [SerializeField] 
    private Rigidbody propellerRb;
    
    [SerializeField] 
    private Rigidbody playerRb;

    [Header("Audio Clips")] 
    [SerializeField]
    private AudioClip startUpClip;
    
    [SerializeField]
    private AudioClip shutdownClip;
    
    [SerializeField]
    private AudioClip propellerClip;
    
    [SerializeField]
    private AudioClip engineClip;
    
    [Header("Sound Players")]
    [SerializeField] 
    private AudioSource ambiencePlayer;
    
    [SerializeField] 
    private AudioSource enginePlayer;
    
    [SerializeField] 
    private AudioSource propellerPlayer;

    [SerializeField] 
    private AudioSource uiPlayer;
    
    [Header("Pitch Control")]
    
    [SerializeField] 
    private float maxPropellerPitch = 1.75f;
    
    [SerializeField]
    private float propellerLerpSpeed = .6f;
    [SerializeField]
    private float engineLerpSpeed = .6f;
    
    [SerializeField]
    private float ambienceSpeed = 3;
    
    private float propellerDefaultVol;
    private float engineDefaultVol;
    

    private bool engineOn;
    private bool startingUp;

    private void Awake()
    {
        uiPlayer.spatialize = false;

        // Propeller noises
        propellerPlayer.playOnAwake = false;
        propellerPlayer.loop = true;
        propellerPlayer.dopplerLevel = 4;
        propellerPlayer.spread = 60;
        propellerPlayer.spatialBlend = .875f;
        propellerPlayer.spatialize = true;

        // Engine noises
        enginePlayer.playOnAwake = false;
        enginePlayer.loop = true;
        enginePlayer.dopplerLevel = 5;
        enginePlayer.spread = 120;
        enginePlayer.spatialize = true;
        enginePlayer.spatialBlend = 1;
        
        // Wind noises
        ambiencePlayer.playOnAwake = false;
        ambiencePlayer.loop = true;
        ambiencePlayer.dopplerLevel = 5;
        ambiencePlayer.spread = 120;
        ambiencePlayer.spatialize = true;
        ambiencePlayer.spatialBlend = 1;
        ambiencePlayer.volume = .7f;
        
        propellerDefaultVol = propellerPlayer.volume;
        engineDefaultVol = enginePlayer.volume;
    }

    private void Start()
    {
        ambiencePlayer.Play();
    }

    private void Update()
    {
        UpdateAmbience();
        UpdatePropeller();
        
        if (!engineOn) return;
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
            
            case ESounds.ThroughHoop:
                PlayThroughHoop();
                break;
        }
    }

    // Change the pitch depending on the speed of the propellers
    void UpdatePropeller()
    {
        if(!engineOn) return;

        float bladeSpeed = propellerRb.angularVelocity.magnitude;
        propellerPlayer.pitch = Mathf.Lerp(.95f, maxPropellerPitch, (bladeSpeed / (engineOn ? propellerRb.maxAngularVelocity : 1)) * propellerLerpSpeed);
    }

    // Change the pitch depending on the speed of the aviator.
    void UpdateAmbience()
    {
        float speed = playerRb.linearVelocity.magnitude;
        float maxSpeed = 320;
        float percentage = speed / maxSpeed;
        ambiencePlayer.volume = Mathf.Lerp(ambiencePlayer.volume, percentage * .8f, Time.deltaTime * ambienceSpeed);
    }
    
    // Change the pitch depending on the speed of the propellers
    void UpdateEngine()
    {
        float bladeSpeed = propellerRb.angularVelocity.magnitude;
        enginePlayer.pitch = Mathf.Lerp(1, 1.75f, (bladeSpeed / (engineOn ? propellerRb.maxAngularVelocity : 1)) * engineLerpSpeed);
    }
    
    private void PlayThroughHoop() { uiPlayer.Play(); }
    
    private void PlayStartEngine()
    {
        if(startingUp) return;
        
        startingUp = true;
        engineOn = true;
        
        // Play the startup sound from the propeller audio source
        propellerPlayer.loop = false;
        propellerPlayer.clip = startUpClip;
        propellerPlayer.Play();
        
        // Swap back to the propeller sound clip after the startup sound finishes
        StartCoroutine(Delay(startUpClip.length, () =>
        {
            propellerPlayer.clip = propellerClip;
            propellerPlayer.loop = true;
            propellerPlayer.Play();
            startingUp = false;
        }));
        
        StartCoroutine(FadeInSound(ESounds.Engine, startUpClip.length + 1.5f));
    }

    
    IEnumerator Delay(float delay, Action function)
    {
        yield return new WaitForSeconds(delay);
        function?.Invoke();
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
                targetVolume = propellerDefaultVol;
                break;
            
            case ESounds.Engine:
                player = enginePlayer;
                targetVolume = engineDefaultVol;
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
        if(startingUp) return;

        propellerPlayer.clip = shutdownClip;
        propellerPlayer.loop = false;
        propellerPlayer.Play();
        engineOn = false;
    }
    
}
