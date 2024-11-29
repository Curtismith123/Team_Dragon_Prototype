using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private int damage;
    private Rigidbody rb;
    private float destroyTime;

    private GameObject attacker;

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
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage damageable = other.GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.takeDamage(damage, attacker);
        }

        Destroy(gameObject);
    }
}
