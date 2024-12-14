using System.Collections;
using UnityEngine;

public class Geyser : MonoBehaviour
{
    [SerializeField] private ParticleSystem geyserParticleSystem;
    [SerializeField] private float intervalDuration = 10f; // Duration of each interval

    private bool isActive = false;
    private Coroutine damageCoroutine;
    private TrapDamage trapDamage;
  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trapDamage = GetComponent<TrapDamage>(); // Get the TrapDamage component
        if (trapDamage == null) 
        {
            Debug.LogError("TrapDamage component not found on " + gameObject.name);
        }
        StartCoroutine(GeyserCycle());
    }

    
    private IEnumerator GeyserCycle()
    {
        while (true)
        {
            isActive = !isActive;
            if (isActive)
            {
                geyserParticleSystem.Play(); // Activate the particle system
                Debug.Log("Geyser activated.");
            } 
            else
            {
                geyserParticleSystem.Stop(); // Deactivate the particle system
                Debug.Log("Geyser deactivated.");
                
            }
            yield return new WaitForSeconds(intervalDuration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(ApplyDamage(other.gameObject));
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") )
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
                    Debug.Log("Player damaged by geyser: " + player.name);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
