using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class M_DoorController : MonoBehaviour
{
    [Header("--Keyed Ojects--")]
    [SerializeField] private string requiredKeyID; // Key ID required to unlock the door
    private bool isUnlocked = false; // Tracks if the door is unlocked
    public GameObject locked;
    public GameObject unlocked;

    [Header("--Audio--")]
    public AudioSource doorSource;
    [SerializeField] AudioClip lockedClip;
    [SerializeField] AudioClip unlockedClip;
    [SerializeField] AudioClip openClip;

    [Header("--Animation--")]

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        locked.SetActive(false);
        unlocked.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isUnlocked) // Check if the door is locked
            {
                // Attempt to unlock the door using the key
                if (gameManager.instance.UseKey(requiredKeyID))
                {
                    isUnlocked = true; // Mark the door as unlocked
                    StartCoroutine(ShowLockState());
                    PlaySound(unlockedClip);
                    Debug.Log("Door unlocked!");
                }
                else
                {
                    StartCoroutine(ShowLockState());
                    PlaySound(lockedClip);
                    return; // Exit without triggering the animation
                }
            }

            // Play the open animation
            anim.SetBool("Open", true);

            PlaySound(openClip);
        }
    }

    private IEnumerator ShowLockState()
    {
        GameObject toShow = isUnlocked ? unlocked : locked;

        toShow.SetActive(true);
        yield return new WaitForSeconds(2f);
        toShow.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isUnlocked)
        {
            // Play the close animation
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
