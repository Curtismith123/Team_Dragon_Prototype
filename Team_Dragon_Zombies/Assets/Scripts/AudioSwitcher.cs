using UnityEngine;

public class AudioSwitcher : MonoBehaviour
{


    public AudioSource AudioSource;

    public AudioClip AudioClip;
    private bool hasTriggered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioSource = GetComponentInParent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            if (AudioClip != null)
            {
                AudioSource.resource = AudioClip;
                AudioSource.Play();
            }
        }
    }
}
