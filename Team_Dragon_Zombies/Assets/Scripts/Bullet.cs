using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private int damage;
    private Rigidbody rb;
    private float destroyTime;

    private GameObject attacker;

    public StatusEffectSO statusEffect;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetSpeed(float bulletSpeed)
    {
        speed = bulletSpeed;
    }

    public void SetDestroyTime(float time)
    {
        destroyTime = time;
    }

    public void SetDamage(int bulletDamage)
    {
        damage = bulletDamage;
    }

    public void SetAttacker(GameObject attacker)
    {
        this.attacker = attacker;
    }


    private void Start()
    {
        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody component is missing.");
            return;
        }

        if (speed == 0)
        {
            Debug.LogWarning($"{name}: Speed is not set. Defaulting to 10.");
            speed = 10f;
        }

        rb.linearVelocity = transform.forward * speed;

        if (destroyTime > 0)
        {
            Destroy(gameObject, destroyTime);
        }
        else
        {
            Debug.LogWarning($"{name}: Destroy time is not set. Defaulting to 5 seconds.");
            Destroy(gameObject, 5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage damageable = other.GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.takeDamage(damage, attacker);
        }
        if (statusEffect != null)
        {
            statusEffect.ApplyEffect(other.gameObject);
        }

        Destroy(gameObject);
    }
}
