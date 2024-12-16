using UnityEngine;

public class DestroyObjectOnEnemyDeath : MonoBehaviour
{
    [SerializeField] private GameObject objectToDestroy;

    private enemyMeleeAttack enemyHealth;

    private void Start()
    {
        enemyHealth = GetComponent<enemyMeleeAttack>();
        if (enemyHealth == null)
        {
            //Debug.LogError("No enemyMeleeAttack script found on this GameObject!");
            return;
        }

        enemyHealth.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath()
    {
        if (objectToDestroy != null)
        {
            //Debug.Log($"Destroying object: {objectToDestroy.name}");
            Destroy(objectToDestroy);
        }
        else
        {
            //Debug.LogWarning("No object assigned to destroy.");
        }

        Destroy(this);
    }

}