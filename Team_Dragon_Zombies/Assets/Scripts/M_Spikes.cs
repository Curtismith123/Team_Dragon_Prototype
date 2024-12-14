using System;
using System.Collections;
using UnityEngine;

public class M_Spikes : MonoBehaviour
{
    [Header("---Settings---")]
    public float triggerDelay; // Delay before the spikes start rising
    public float upTime; // Time it takes for spikes to rise
    public float downTime; // Time it takes for spikes to lower
    public float resetDelay; // Delay before the trap can be triggered again
    public float spikeRise; // Height the spikes rise

    [Header("---Object---")]
    public GameObject Spikes; // Reference to the spikes object

    [Header("---Audio---")]
    public AudioClip Click; // Audio when the trap triggers
    public AudioClip SpikesUp; // Audio when spikes rise
    public AudioClip SpikesDown; // Audio when spikes lower

    private AudioSource audioSource; // AudioSource component for playing sounds
    private bool hasTriggered; // To prevent retriggering
    private Vector3 startPos; // Starting position of spikes
    private Vector3 endPos; // Target position when spikes are fully raised

    private void Start()
    {
        // Save the starting position of the spikes
        startPos = Spikes.transform.position;

        // Calculate the raised position based on spikeRise
        endPos = startPos + Vector3.up * spikeRise;

        // Get the AudioSource component attached to the GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource component found! Please attach one to this GameObject.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            StartCoroutine(Spiketrap());
        }
    }

    private IEnumerator Spiketrap()
    {
        hasTriggered = true;

        // Play Click sound when the trap is triggered
        PlaySound(Click);

        // Wait for the trigger delay
        yield return new WaitForSeconds(triggerDelay);

        // Move spikes up
        PlaySound(SpikesUp);
        float elapsedTime = 0f;
        while (elapsedTime < upTime)
        {
            Spikes.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / upTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure spikes reach the target position
        Spikes.transform.position = endPos;

        // Wait for reset delay while spikes remain up
        yield return new WaitForSeconds(resetDelay);

        // Move spikes down
        PlaySound(SpikesDown);
        elapsedTime = 0f;
        while (elapsedTime < downTime)
        {
            Spikes.transform.position = Vector3.Lerp(endPos, startPos, elapsedTime / downTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure spikes return to the starting position
        Spikes.transform.position = startPos;

        // Wait before the trap can be triggered again
        yield return new WaitForSeconds(resetDelay);

        hasTriggered = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

