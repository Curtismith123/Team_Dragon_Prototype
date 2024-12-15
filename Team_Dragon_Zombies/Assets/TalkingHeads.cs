using UnityEngine;
using UnityEngine.Audio;

public class TalkingHeads : MonoBehaviour
{

    public AudioSource AudioSource;
    public AudioClip clip;
    public bool hasPlayed;
    private void Start()
    {
        hasPlayed = false;
        if (AudioSource == null)
        {
            AudioSource = GetComponent<AudioSource>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !hasPlayed)
        {

            PlaySound(clip);
            hasPlayed = true;
        }

    }
    private void PlaySound(AudioClip clip)
    {
        if (AudioSource != null && clip != null)
        {
            AudioSource.PlayOneShot(clip);
        }
    }
}
