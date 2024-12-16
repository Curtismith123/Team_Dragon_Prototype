using UnityEngine;

public class Boulder : MonoBehaviour
{
    [Header("----- Boulder Settings -----")]
    [SerializeField] private int boulderDamage = 50;
    [SerializeField] private float lifetime = 5f;

    private bool hasDealtDamage = false;

    private void Start()
    {
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject, lifetime);
        }
        else
        {
            Destroy(gameObject, lifetime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && !hasDealtDamage)
        {
            IDamage damageable = other.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(boulderDamage, gameObject);
                hasDealtDamage = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasDealtDamage && collision.gameObject.CompareTag("Floor"))
        {
            hasDealtDamage = true;
        }
    }
}
