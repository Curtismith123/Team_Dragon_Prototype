using UnityEngine;

public class HealthPickup : MonoBehaviour
{

    

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (other.CompareTag("Player"))
        {
            player.HealToFull();
            Destroy(gameObject);
        }
    }
}
