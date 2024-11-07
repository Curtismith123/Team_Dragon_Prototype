using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyMeleeAttack : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewAngle;

    [SerializeField] float attackRate = 1.5f; //default 1.5s
    [SerializeField] int meleeDamage = 10; //default 10 dmg
    [SerializeField] float attackRange = 2f; //default 2 range

    bool isAttacking;
    bool playerInRange;

    Color colorOrig;

    Vector3 playerDir;

    float angleToPlayer;

    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);
    }

    void Update()
    {

        if ((playerInRange))
        {
            playerDir = gameManager.instance.player.transform.position - headPos.position;

            agent.SetDestination(gameManager.instance.player.transform.position);

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                faceTarget();
            }

            if (!isAttacking)
            {
                StartCoroutine(MeleeAttack());
            }
        }
    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewAngle)
            {
                //can see player
                agent.SetDestination(gameManager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                if (!isAttacking)
                {
                    StartCoroutine(MeleeAttack());
                }
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator MeleeAttack()
    {
        isAttacking = true;

        if (gameManager.instance.player != null &&
            Vector3.Distance(transform.position, gameManager.instance.player.transform.position) <= attackRange)
        {
            IDamage playerDamage = gameManager.instance.player.GetComponent<IDamage>();
            if (playerDamage != null)
            {
                playerDamage.takeDamage(meleeDamage);
            }
        }

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
}