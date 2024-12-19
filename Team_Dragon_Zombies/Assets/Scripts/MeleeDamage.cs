using System;
using System.Collections;
using UnityEngine;

public class MeleeDamage : MonoBehaviour
{
    public int damageAmount = 10;
    private PlayerController player;
    private bool canHit;
    public float delay;

    private void Start()
    {
        player = gameManager.instance.playerScript;
        canHit = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && player != null && canHit)
        {

            StartCoroutine(doDamage());
            canHit = true;
        }
    }

    private IEnumerator doDamage()
    {

        player.takeDamage(damageAmount, gameObject);
        yield return new WaitForSeconds(delay);

    }

}

