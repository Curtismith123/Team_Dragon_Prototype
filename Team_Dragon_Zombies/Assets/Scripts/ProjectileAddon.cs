using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    public int damage; 

    private Rigidbody rb;
    private bool targetHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // make sure only to stick to the first target you hit
        if (targetHit)
            return;
        else
            targetHit = true;

        // check if you hit an Object
        if (collision.gameObject.GetComponent<ObjectDamage>() != null)
        {
            ObjectDamage enemy = collision.gameObject.GetComponent<ObjectDamage>();

            enemy.takeDamage(damage);

            Destroy(gameObject);
        }

        // make sure projectile sticks to surface
        rb.isKinematic = true;

        // make sure projectile moves with target
        transform.SetParent(collision.transform);
    }

}
