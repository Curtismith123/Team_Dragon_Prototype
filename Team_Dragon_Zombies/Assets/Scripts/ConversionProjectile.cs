using UnityEngine;

public class ConversionProjectile : MonoBehaviour
{
    private float speed;
    private Rigidbody rb;
    private float destroyTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    public void SetSpeed(float projectileSpeed)
    {
        speed = projectileSpeed;
    }

    public void SetDestroyTime(float time)
    {
        destroyTime = time;
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        if (other.CompareTag("Player") || other.CompareTag("Friendly"))
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            var enemyScript = other.GetComponent<enemyMeleeAttack>();
            if (enemyScript != null)
            {
                enemyScript.StartConversion();
            }
            else
            {
                Debug.LogWarning($"{other.name} does not have an enemyMeleeAttack script attached.");
            }
        }
        Destroy(gameObject);
    }
}
