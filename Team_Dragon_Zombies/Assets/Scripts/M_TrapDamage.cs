using System.Collections;
using UnityEngine;

public class M_TrapDamage : MonoBehaviour
{
    public int damageAmount = 10;
    private PlayerController player;
    private bool canDamage = true;

    private void Start()
    {
        player = gameManager.instance.playerScript;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canDamage && player != null)
        {
            player.takeDamage(damageAmount, gameObject);
            StartCoroutine(DamageCooldown(1f));
        }
    }

    private IEnumerator DamageCooldown(float cooldownTime)
    {
        canDamage = false;
        yield return new WaitForSeconds(cooldownTime);
        canDamage = true;
    }
}

