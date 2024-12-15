using UnityEngine;

public class AudioSwitcher : MonoBehaviour
{


    public AudioSource AudioSource;

    public AudioClip AudioClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioSource = GetComponentInParent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (AudioClip != null)
            {
                AudioSource.resource = AudioClip;
                AudioSource.Play();
            }
        }
    }
}
