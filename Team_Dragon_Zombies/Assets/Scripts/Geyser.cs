using System.Collections;
using UnityEngine;

public class Geyser : MonoBehaviour
{
    [SerializeField] private ParticleSystem geyserParticleSystem;
    [SerializeField] private float intervalDuration = 10f; // Duration of each interval

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         StartCoroutine(GeyserCycle());
    }

    
    private IEnumerator GeyserCycle()
    {
        bool isActive = false;

        while (true)
        {
            if (isActive)
            {
                geyserParticleSystem.Stop(); // Deactivate the particle system
                Debug.Log("Geyser deactivated.");
            } 
            else
            {
                geyserParticleSystem.Play(); // Activate the particle system
                Debug.Log("Geyser activated.");
            }
            isActive = !isActive;
            yield return new WaitForSeconds(intervalDuration);
        }
    }
}
