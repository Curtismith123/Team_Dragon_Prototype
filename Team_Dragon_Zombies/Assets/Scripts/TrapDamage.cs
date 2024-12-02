using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public int damageAmount = 10; // Amount of damage the trap deals
    private bool playerHit = false; // Track if the the player has been hit

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !playerHit)
        {
            IDamage damageable = collision.gameObject.GetComponent<IDamage>();
            if (damageable != null) 
            { 
                damageable.takeDamage(damageAmount,gameObject);
                playerHit = true;
                Debug.Log("Player hit" + collision.gameObject.name);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerHit = false;
            Debug.Log("Player exited: " + collision.gameObject.name);
        }
    }
}
