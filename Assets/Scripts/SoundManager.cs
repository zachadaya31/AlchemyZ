using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    public AudioSource audioSourceButton;
    public AudioSource audioSourceSpeak;

    [Header("Button Sounds")]
    public AudioClip soundButtonPress;

    [Header("Dialogue Sounds")]
    public AudioClip soundDialoueSpeak;

    System.Random rdm = new System.Random();


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playSoundButtonPress()
    {
        audioSourceButton.PlayOneShot(soundButtonPress);
    }

    public void playSoundDialogueSpeak()
    {
        float randomPitch = rdm.Next(3,4);
        audioSourceSpeak.pitch = randomPitch;

        audioSourceSpeak.PlayOneShot(soundDialoueSpeak);
    }
}
