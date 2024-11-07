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
    [SerializeField] private float attackRate = 1.5f; //default 1.5s
    [SerializeField] private int meleeDamage = 10; //default 10 dmg
    [SerializeField] private float attackRange = 2f; //default 2 range

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
        if (gameManager.instance.player == null)
        {
            playerInRange = false;
            return;
        }

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

        agent.SetDestination(gameManager.instance.player.transform.position);

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

    private void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }
}
