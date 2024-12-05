using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class FriendlyAI : MonoBehaviour, IDamage, IFriendly
{
    [Header("-----References-----")]
    public NavMeshAgent agent;
    public Animator anim;
    public Renderer model;
    public Transform headPos;

    [Header("-----Stats-----")]
    public int HP;
    public float faceTargetSpeed;
    public float detectionRadius = 10f;
    public int animSpeedTrans;
    public float attackRate = 1.5f;
    public int meleeDamage = 10;
    public float attackRange = 2f;
    public float followRadius = 5f;
    public float idleRoamRadius = 3f;
    public float minIdleRoamInterval = 2f;
    public float maxIdleRoamInterval = 5f;
    public float playerMoveThreshold = 0.2f;

    [Header("-----Audio-----")]
    public AudioClip walkSound;
    public AudioClip attackSound;
    [Range(0, 1)] public float walkSoundVolume = 0.5f;
    [Range(0, 1)] public float attackSoundVolume = 1f;

    private AudioSource audioSource;
    private GameObject player;

    private bool isAttacking;
    private bool isDead = false;
    private bool isRoaming = false;

    private List<Color> originalColors = new List<Color>();
    private List<Renderer> renderers = new List<Renderer>();

    private GameObject currentEnemy;

    private Vector3 followOffset;
    private float lastIdleRoamTime = 0f;
    private float idleRoamInterval;

    private Vector3 lastPlayerPosition;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = agent ?? GetComponent<NavMeshAgent>();
        anim = anim ?? GetComponent<Animator>();

        if (player == null || agent == null || anim == null)
        {
            Debug.LogWarning($"FriendlyAI {name} is missing required components.");
        }

        agent.radius = 0.5f;
        agent.stoppingDistance = 0.5f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 70);

        agent.updateRotation = false;

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Friendly"), LayerMask.NameToLayer("Enemy"));

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        renderers.Clear();
        originalColors.Clear();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderers.Add(renderer);
            renderer.material = new Material(renderer.material);
            originalColors.Add(renderer.material.color);
        }
        ApplyGreenTint();

        gameManager.instance.RegisterFriendly(this);

        lastIdleRoamTime = Time.time + Random.Range(0f, maxIdleRoamInterval);
        idleRoamInterval = Random.Range(minIdleRoamInterval, maxIdleRoamInterval);
        lastPlayerPosition = player.transform.position;
    }

    void ApplyGreenTint()
    {
        Color greenTint = Color.green;

        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.color = greenTint;
            originalColors[i] = greenTint;
        }
    }

    void Update()
    {
        float agentSpeed = agent.velocity.magnitude;
        float animSpeed = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(animSpeed, agentSpeed, Time.deltaTime * animSpeedTrans));

        if (Vector3.Distance(player.transform.position, lastPlayerPosition) > playerMoveThreshold)
        {
            if (!isAttacking)
            {
                isRoaming = false;
                StopAllCoroutines();
                FollowPlayer();
            }
        }

        lastPlayerPosition = player.transform.position;

        if (canSeeEnemy())
        {
            agent.stoppingDistance = attackRange - 0.1f;
            agent.SetDestination(currentEnemy.transform.position);

            FaceTarget(currentEnemy.transform.position);

            if (!isAttacking && agent.remainingDistance <= attackRange)
            {
                StartCoroutine(Melee());
            }
        }
        else
        {
            if (!isAttacking && !isRoaming)
            {
                FollowPlayer();
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!isRoaming && Time.time - lastIdleRoamTime > idleRoamInterval)
                {
                    StartCoroutine(IdleRoam());
                }
            }
            else
            {
                if (agent.velocity.sqrMagnitude > 0.1f)
                {
                    FaceDirection(agent.velocity.normalized);
                }
            }
        }

        if (agent.velocity.magnitude > 0.1f && !audioSource.isPlaying)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.volume = walkSoundVolume;
            audioSource.Play();
        }
        else if (agent.velocity.magnitude < 0.1f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void AssignFollowOffset(int index, int totalFriendlies)
    {
        if (totalFriendlies <= 0)
        {
            totalFriendlies = 1;
        }

        float angle = index * Mathf.PI * 2 / totalFriendlies;

        float x = Mathf.Cos(angle) * followRadius;
        float z = Mathf.Sin(angle) * followRadius;
        followOffset = new Vector3(x, 0, z);
    }

    void FollowPlayer()
    {
        agent.stoppingDistance = 0.5f;
        Vector3 followPosition = player.transform.position + followOffset;
        agent.SetDestination(followPosition);
    }

    IEnumerator IdleRoam()
    {
        isRoaming = true;
        lastIdleRoamTime = Time.time;

        Vector2 randomCircle = Random.insideUnitCircle * idleRoamRadius;
        Vector3 offsetPosition = player.transform.position + followOffset;
        Vector3 roamPosition = offsetPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(roamPosition, out hit, idleRoamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.stoppingDistance = 0.5f;
        }

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            if (Vector3.Distance(player.transform.position, lastPlayerPosition) > playerMoveThreshold)
            {
                isRoaming = false;
                yield break;
            }
            yield return null;
        }

        idleRoamInterval = Random.Range(minIdleRoamInterval, maxIdleRoamInterval);
        isRoaming = false;
    }

    bool canSeeEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float shortestDistance = detectionRadius;
        GameObject nearestEnemy = null;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                GameObject enemy = collider.gameObject;
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }
        }

        if (nearestEnemy != null)
        {
            currentEnemy = nearestEnemy;
            return true;
        }
        else
        {
            currentEnemy = null;
            return false;
        }
    }

    IEnumerator Melee()
    {
        isAttacking = true;
        anim.SetTrigger("Melee");
        audioSource.PlayOneShot(attackSound, attackSoundVolume);

        yield return new WaitForSeconds(0.1f);

        if (currentEnemy != null && Vector3.Distance(transform.position, currentEnemy.transform.position) <= attackRange)
        {
            IDamage targetDamage = currentEnemy.GetComponent<IDamage>();
            targetDamage?.takeDamage(meleeDamage, gameObject);
        }

        yield return new WaitForSeconds(attackRate - 0.1f);
        isAttacking = false;
    }

    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        }
    }

    void FaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        }
    }

    public void takeDamage(int amount, GameObject attacker)
    {
        if (isDead) return;

        HP -= amount;
        StartCoroutine(FlashRed());

        if (attacker != null)
        {
            currentEnemy = attacker;
        }

        if (HP <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        gameManager.instance.RemoveFriendly(this);
        Destroy(gameObject);
    }

}
