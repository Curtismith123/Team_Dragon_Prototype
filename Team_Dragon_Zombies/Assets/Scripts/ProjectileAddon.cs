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

        // check if you hit an enemy
        if (collision.gameObject.GetComponent<BasicEnemy>() != null)
        {
            BasicEnemy enemy = collision.gameObject.GetComponent<BasicEnemy>();

            enemy.takeDamage(damage);

            Destroy(gameObject);
        }

        // make sure projectile sticks to surface
        rb.isKinematic = true;

        // make sure projectile moves with target
        transform.SetParent(collision.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (targetHit && rb.isKinematic)
        {
            rb.MovePosition(transform.position);
        }
    }
}
