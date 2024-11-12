using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damageAmount;
    private float speed;
    private Rigidbody rb;
    private float destroyTime = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetDamage(int damage)
    {
        damageAmount = damage;
    }

    public void SetSpeed(float bulletSpeed)
    {
        speed = bulletSpeed;
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

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            dmg.takeDamage(damageAmount);
        }

        Destroy(gameObject);
    }
}
