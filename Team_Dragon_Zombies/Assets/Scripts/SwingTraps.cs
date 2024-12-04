using System.Collections;
using UnityEngine;

public class SwingTraps : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip activateSound;
    public float activateDelay;

    private TrapDamage trapDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        trapDamage = GetComponent<TrapDamage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered log trap trigger: " + other.gameObject.name);
            StartCoroutine(ActivatieLogWithDelay());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited log trap trigger: " + other.gameObject.name);
        }
    }

    IEnumerator ActivatieLogWithDelay()
    {
        yield return new WaitForSeconds(activateDelay);

        if (audioSource != null && activateSound != null)
        {
            audioSource.PlayOneShot(activateSound);
        }
    }

    private void DealDamageToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && trapDamage != null)
        {
            IDamage damageable = player.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(trapDamage.damageAmount, gameObject);
                Debug.Log("Player damaged by log trap: " + player.name);
            }

        }
    }
}
