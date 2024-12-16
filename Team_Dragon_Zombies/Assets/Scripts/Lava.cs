using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("----- Lava Settings -----")]
    [SerializeField] private int lavaDamage = 9999;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            IDamage damageable = other.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(lavaDamage, gameObject);
            }
        }
    }
}
