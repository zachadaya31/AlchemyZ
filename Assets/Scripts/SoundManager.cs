using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("BUTTON SOUNDS")]
    public AudioSource nextButtonSound;
    public AudioSource backButtonSound;

    [Header("DIALOGUE SOUNDS")]
    public AudioSource dialogueLineSound;
    public AudioClip dialogueLineClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    public float maxVolume;

    [Header("Text Sound")]
    public AudioSource textSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playSoundNextButton() 
    { 
        nextButtonSound.Play();
    }
    public void playSoundBackButton()
    {
        backButtonSound.Play();
    }


    public void playSoundDialogueLine() 
    { 
        float randomPitch = Random.Range(minPitch, maxPitch);
        dialogueLineSound.pitch = randomPitch;
        
        float randomVolume = Random.Range(maxVolume-0.05f, maxVolume);

        dialogueLineSound.PlayOneShot(dialogueLineClip, randomVolume);
    }

    public void playTextSound() {
        textSound.Play();
    }
}
