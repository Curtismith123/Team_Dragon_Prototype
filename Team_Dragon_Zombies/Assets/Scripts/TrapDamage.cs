using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public int damageAmount = 10; // Amount of damage the trap deals

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IDamage damageable = collision.gameObject.GetComponent<IDamage>();
            if (damageable != null) 
            { 
                damageable.takeDamage(damageAmount,gameObject);

            }
        }
    }
}
