using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damage : MonoBehaviour
{

    enum damageType { bullet, stationary }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

<<<<<<< Updated upstream
=======

    // Start is called once before the first execution of Update after the MonoBehaviour is created
>>>>>>> Stashed changes
    void Start()
    {
        if(type == damageType.bullet)
        {
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null )
        {
            dmg.takeDamage(damageAmount);
        }

        if(type == damageType.bullet)
        {
            Destroy(gameObject);
        }
    }
}
