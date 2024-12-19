using UnityEngine;

public class UnlockedDoors : MonoBehaviour
{

    private Animator anim;
    private AudioSource doorSource;
    public AudioClip clip;
    private bool isOpen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        doorSource = GetComponent<AudioSource>();
        isOpen = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isOpen = true;
            anim.SetBool("Open", true);
            PlaySound(clip);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            anim.SetBool("Open", false);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (doorSource != null && clip != null)
        {
            doorSource.PlayOneShot(clip);
        }
    }
}
