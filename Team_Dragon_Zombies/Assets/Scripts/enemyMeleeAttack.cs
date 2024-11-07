using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyMeleeAttack : MonoBehaviour, IDamage
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPos;
    [SerializeField] private int HP;
    [SerializeField] private int faceTargetSpeed;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float attackRange = 2f;

    private bool isAttacking;
    private bool playerInRange;

    private Color colorOrig;
    private Vector3 playerDir;

    private void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);

        agent.stoppingDistance = attackRange;
    }

    private void Update()
    {
        if (playerInRange)
        {
            playerDir = gameManager.instance.player.transform.position - headPos.position;
            agent.SetDestination(gameManager.instance.player.transform.position);

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                faceTarget();

                if (!isAttacking)
                {
                    StartCoroutine(MeleeAttack());
                }
            }
        }
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

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
    }

    private IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    private IEnumerator MeleeAttack()
    {
        isAttacking = true;

        if (Vector3.Distance(transform.position, gameManager.instance.player.transform.position) <= attackRange)
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

    private void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
}
