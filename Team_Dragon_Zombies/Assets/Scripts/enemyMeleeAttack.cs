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
    [SerializeField] int meleeDamage = 10;    //default 10 dmg
    [SerializeField] float attackRange = 2f;  //default 2 range

    [SerializeField] AudioClip walkSound;
    [SerializeField] AudioClip attackSound;
    [SerializeField][Range(0, 1)] float walkSoundVolume = 0.5f;
    [SerializeField][Range(0, 1)] float attackSoundVolume = 1f;

    private AudioSource audioSource;

    bool isAttacking;
    bool isRoaming;
    bool isDead = false;

    public delegate void OnDeathEvent();
    public event OnDeathEvent OnDeath;

    Color colorOrig;
    List<Renderer> renderers = new List<Renderer>();

    Vector3 targetDir;
    Vector3 startingPos;

    float angleToTarget;
    float stoppingDistOrig;

    [Header("-----Enemy Conversion-----")]
    [SerializeField] EnemyTier enemyTier;
    [SerializeField] float conversionTime;
    private float conversionTimer = 0f;
    private bool isConverting = false;

    private GameObject target;

    //variables for target switching
    [SerializeField] float detectionRadius = 15f;
    private float timeSinceLastHit = Mathf.Infinity;
    [SerializeField] float timeToSwitchTarget = 3f;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        anim.applyRootMotion = false;

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

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
        float agentSpeed = agent.velocity.magnitude;
        float animSpeed = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(animSpeed, agentSpeed, Time.deltaTime * animSpeedTrans));

        timeSinceLastHit += Time.deltaTime;

        if (target != null)
        {
            if (canSeeTarget())
            {
                targetDir = target.transform.position - headPos.position;
                agent.SetDestination(target.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                if (!isAttacking)
                {
                    StartCoroutine(melee());
                }
            }
            else
            {
                //can't see target, switch if necessary
                if (timeSinceLastHit >= timeToSwitchTarget)
                {
                    FindClosestTarget();
                }
            }
        }
        else
        {
            if (timeSinceLastHit >= timeToSwitchTarget)
            {
                FindClosestTarget();
            }

            if (target == null && !isRoaming && agent.remainingDistance < 0.05f)
            {
                StartCoroutine(roam());
            }
        }

        HandleAudio();
        HandleConversion();
    }

    void HandleAudio()
    {
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

    void HandleConversion()
    {
        if (isConverting)
        {
            if (target != null && target.CompareTag("Player"))
            {
                conversionTime -= Time.deltaTime;
                if (conversionTime <= 0)
                {
                    ConvertToFriendly();
                }
            }
            else
            {
                Debug.Log($"Conversion of {name} stopped. Player not in range.");
                isConverting = false;
            }
        }
    }

    IEnumerator roam()
    {
        isRoaming = true;
        yield return new WaitForSeconds(roamTimer);

        agent.stoppingDistance = 0;
        Vector3 randomDist = Random.insideUnitSphere * roamDist + startingPos;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDist, out hit, roamDist, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        isRoaming = false;
    }

    bool canSeeTarget()
    {
        if (target == null)
            return false;

        targetDir = target.transform.position - headPos.position;
        angleToTarget = Vector3.Angle(targetDir, transform.forward);

        if (Vector3.Distance(transform.position, target.transform.position) > detectionRadius)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, targetDir, out hit, detectionRadius))
        {
            if ((hit.collider.CompareTag("Player") ||
                 hit.collider.CompareTag("Friendly") ||
                 hit.collider.CompareTag("FriendlySpewer")) &&
                 angleToTarget <= viewAngle)
            {
                return true;
            }
        }

        return false;
    }

    void FindClosestTarget()
    {
        timeSinceLastHit = 0f;
        float shortestDistance = detectionRadius;
        GameObject nearestTarget = null;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player") ||
                collider.CompareTag("Friendly") ||
                collider.CompareTag("FriendlySpewer"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = collider.gameObject;
                }
            }
        }

        if (nearestTarget != null)
        {
            target = nearestTarget;
        }
        else
        {
            target = null;
            StartCoroutine(roam());
        }
    }

    public void takeDamage(int amount, GameObject attacker)
    {
        if (isDead) return;

        HP -= amount;
        StartCoroutine(flashRed());

        timeSinceLastHit = 0f;
        target = attacker;

        if (HP <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
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
        audioSource.PlayOneShot(attackSound, attackSoundVolume);

        yield return new WaitForSeconds(0.1f);

        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            IDamage targetDamage = target.GetComponent<IDamage>();
            targetDamage?.takeDamage(meleeDamage, gameObject);
        }

        yield return new WaitForSeconds(attackRate - 0.1f);
        isAttacking = false;
    }

    void faceTarget()
    {
        if (targetDir != Vector3.zero)
        {
            Vector3 direction = new Vector3(targetDir.x, 0, targetDir.z);
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        }
    }

    public void StartConversion()
    {
        if (!isConverting)
        {
            isConverting = true;
            AssignConversionTime();
            conversionTimer = conversionTime;

            if (enemyTier == EnemyTier.Tier1)
            {
                ConvertToFriendly();
                isConverting = false;
            }
        }
    }

    private void AssignConversionTime()
    {
        switch (enemyTier)
        {
            case EnemyTier.Tier1:
                conversionTime = 0f;
                break;
            case EnemyTier.Tier2:
                conversionTime = Random.Range(5f, 10f);
                break;
            case EnemyTier.Tier3:
                conversionTime = Random.Range(15f, 20f);
                break;
        }
    }

    void ConvertToFriendly()
    {
        gameObject.tag = "Friendly";

        int friendlyLayer = LayerMask.NameToLayer("Friendly");
        if (friendlyLayer != -1)
        {
            gameObject.layer = friendlyLayer;
        }

        FriendlyAI friendlyAI = gameObject.AddComponent<FriendlyAI>();

        //transfer stats
        friendlyAI.HP = this.HP;
        friendlyAI.faceTargetSpeed = this.faceTargetSpeed;
        friendlyAI.animSpeedTrans = this.animSpeedTrans;
        friendlyAI.attackRate = this.attackRate;
        friendlyAI.meleeDamage = this.meleeDamage;
        friendlyAI.attackRange = this.attackRange;

        //transfer references
        friendlyAI.agent = this.agent;
        friendlyAI.model = this.model;
        friendlyAI.headPos = this.headPos;
        friendlyAI.anim = this.anim;
        friendlyAI.walkSound = this.walkSound;
        friendlyAI.attackSound = this.attackSound;
        friendlyAI.walkSoundVolume = this.walkSoundVolume;
        friendlyAI.attackSoundVolume = this.attackSoundVolume;

        gameManager.instance.updateGameGoal(-1);

        Destroy(this);
    }
}
