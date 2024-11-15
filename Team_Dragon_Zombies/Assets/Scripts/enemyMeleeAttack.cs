using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyMeleeAttack : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator anim;
    [SerializeField] Transform headPos;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewAngle;
    [SerializeField] int roamDist;
    [SerializeField] int roamTimer;
    [SerializeField] int animSpeedTrans;

    [SerializeField] float attackRate = 1.5f; //default 1.5s
    [SerializeField] int meleeDamage = 10; //default 10 dmg
    [SerializeField] float attackRange = 2f; //default 2 range

    bool isAttacking;
    bool playerInRange;
    bool isRoaming;
    bool isDead = false;

    Color colorOrig;
    List<Renderer> renderers = new List<Renderer>();

    Vector3 playerDir;
    Vector3 startingPos;

    float angleToPlayer;
    float stoppingDistOrig;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderers.Add(renderer);
            colorOrig = renderer.material.color;
        }
        gameManager.instance.updateGameGoal(1);
        stoppingDistOrig = agent.stoppingDistance;
        startingPos = transform.position;
    }

    void Update()
    {
        float agentSpeed = agent.velocity.normalized.magnitude;
        float animSpeed = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.Lerp(animSpeed, agentSpeed, Time.deltaTime * animSpeedTrans));

        if (playerInRange && canSeePlayer())
        {
            playerDir = gameManager.instance.player.transform.position - headPos.position;

            agent.SetDestination(gameManager.instance.player.transform.position);

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                faceTarget();
            }

            if (!isAttacking)
            {
                StartCoroutine(melee());
            }
        }

        if (playerInRange && !canSeePlayer())
        {
            if (!isRoaming && agent.remainingDistance < 0.05f)
                StartCoroutine(roam());
        }
        else if (!playerInRange)
        {
            if (!isRoaming && agent.remainingDistance < 0.05f)
                StartCoroutine(roam());
        }
    }

    IEnumerator roam()
    {
        isRoaming = true;
        yield return new WaitForSeconds(roamTimer);

        agent.stoppingDistance = 0;
        Vector3 randomDist = Random.insideUnitSphere * roamDist;
        randomDist += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDist, out hit, roamDist, 1);
        agent.SetDestination(hit.position);

        isRoaming = false;
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
                    StartCoroutine(melee());
                }
                return true;
            }
        }
        agent.stoppingDistance = 0;

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
        if (isDead) return;

        HP -= amount;
        StartCoroutine(flashRed());

        agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            isDead = true;
            gameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = colorOrig;
        }
    }

    IEnumerator melee()
    {
        isAttacking = true;
        anim.SetTrigger("Melee");

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