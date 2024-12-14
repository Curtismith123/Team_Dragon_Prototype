using System.Collections;
using UnityEngine;

public class TrapSpikesAct : MonoBehaviour
{
    
    public AudioSource audioSource;
    public AudioClip activateSound;
    public float activationDelay = 1f; // Delay before spikes activate
    public float spikeHeight = 1f; // Height to which the spikes will rise
    public float spikeSpeed = 2f; // Speed at which the spike will rise
    public float intervalDuration = 10f; // Duration of each interval

    public Vector3 initialPosition;
    public bool isActivated = false;

    private TrapDamage trapDamage;
    private Coroutine damageCoroutine;

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

        trapDamage = GetComponent<TrapDamage>();
        if (trapDamage == null)
        {
            Debug.LogError("TrapDamage component not found on " + gameObject.name);
        }
        StartCoroutine(SpikeCycle());
    }

    private IEnumerator SpikeCycle()
    {
        while (true)
        {
            isActivated = !isActivated;
            if (isActivated)
            {
                Debug.Log("Spikes activated.");
                StartCoroutine(MoveSpikes(initialPosition + Vector3.up * spikeHeight));

                // Play the activation sound
                if (audioSource != null && activateSound != null)
                {
                    audioSource.PlayOneShot(activateSound);
                }
            } 
            else
            {
                Debug.Log("Spikes deactivated.");
                StartCoroutine(MoveSpikes(initialPosition));

                StopDamageCoroutine();
            }
            yield return new WaitForSeconds(intervalDuration);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActivated)
        {
           if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(ApplyDamage(other.gameObject));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopDamageCoroutine();
        }
    }

    private void StopDamageCoroutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator ApplyDamage(GameObject player)
    {
        while (true)
        {
            if (trapDamage != null)
            {
                IDamage damageable = player.GetComponent<IDamage>();
                if (damageable != null)
                {
                    damageable.takeDamage(trapDamage.damageAmount, gameObject);
                    Debug.Log("Player damaged by spikes" + player.name);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }


    IEnumerator MoveSpikes(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, spikeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
