using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int health;

    public void takeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Destroy(gameObject);
    }
}
