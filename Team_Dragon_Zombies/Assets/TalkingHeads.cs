using UnityEngine;
using UnityEngine.Audio;

public class TalkingHeads : MonoBehaviour
{

    public AudioSource AudioSource;
    public AudioClip clip;
    private bool hasPlayed = false;
    private void Start()
    {
        if (AudioSource == null)
        {
            AudioSource = GetComponent<AudioSource>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !hasPlayed)
            PlaySound(clip);

    }
    private void PlaySound(AudioClip clip)
    {
        if (AudioSource != null && clip != null)
        {
            AudioSource.PlayOneShot(clip);
        }
    }
}
