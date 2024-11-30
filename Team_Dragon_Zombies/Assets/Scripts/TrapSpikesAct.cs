using System.Collections;
using UnityEngine;

public class TrapSpikesAct : MonoBehaviour
{
    
    public AudioSource audioSource;
    public AudioClip activateSound;
    public float activationDelay = 1f; // Delay before spikes activate
    public float spikeHeight = 1f; // Height to which the spikes will rise
    public float spikeSpeed = 2f; // Speed at which the spike will rise

    public Vector3 initialPosition;
    public bool isActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // Get the AudioSource component if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        // Record the initial position of the spikes 
       initialPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ActivateSpikeWithDelay());
        }
    }

    IEnumerator ActivateSpikeWithDelay()
    {
        // Wait for the activation delay
        yield return new WaitForSeconds(activationDelay);

        // Play the activation sound 
        if (audioSource != null && activateSound != null)
        {
            audioSource.PlayOneShot(activateSound);
        }

        // Activate the spikes
        isActivated = true;
        StartCoroutine(MoveSpikes(initialPosition + Vector3.up * spikeHeight));
    }

    IEnumerator MoveSpikes(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, spikeSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Deactivate the spike after a short delay
        yield return new WaitForSeconds(2f);
        StartCoroutine(MoveSpikes(initialPosition));
        isActivated = false;
    }
}
