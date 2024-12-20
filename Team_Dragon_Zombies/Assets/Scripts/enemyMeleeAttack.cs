using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static StatusEffectSO;

public class enemyMeleeAttack : MonoBehaviour, IDamage
{
    [Header("---SetTrue If Boss or miniBoss")]
    public bool isBoss;
    [SerializeField] public float rangeTuner;
    [SerializeField] public GameObject hitPoint;
    [SerializeField] GameObject dropItem;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator anim;
    [SerializeField] Transform headPos;

    [SerializeField] public int HP;
    [SerializeField] public float faceTargetSpeed;
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

    [Header("-------Elemental Weak/Res------- ")]
    [Header("--0= Weak 1=Default 2=Resistant-")]

    [Range(0, 2)] public int fireResistance;
    [Range(0, 2)] public int iceResistance;
    [Range(0, 2)] public int lightningResistance;

    // Base multiplier for tier scaling (T4 takes less damage from weaknesses)
    private float[] tierMultipliers = { 1f, 0.75f, 0.5f, 0.25f };

    bool isAttacking;
    bool isRoaming;
    public bool isDead = false;

    public delegate void OnDeathEvent();
    public event OnDeathEvent OnDeath;

    Color colorOrig;
    List<Renderer> renderers = new List<Renderer>();

    Vector3 targetDir;
    Vector3 startingPos;

    float angleToTarget;
    float stoppingDistOrig;
    private float originalSpeed;

    [Header("-----Enemy Conversion-----")]
    [SerializeField] public EnemyTier enemyTier;
    [SerializeField] float conversionTime;
    private float conversionTimer = 0f;
    private bool isConverting = false;
    private bool isStunned = false;

    private GameObject target;

    //variables for target switching
    [SerializeField] float detectionRadius = 15f;
    private float timeSinceLastHit = Mathf.Infinity;
    [SerializeField] float timeToSwitchTarget = 3f;

    [SerializeField] GameObject popupDamagePrefab;
    [SerializeField] TMP_Text popupDamage;

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
        stoppingDistOrig = 0 + rangeTuner;
        startingPos = transform.position;
        originalSpeed = agent.speed;
    }

    void Update()
    {
        if (isBoss)
        {
            agent.SetDestination(gameManager.instance.playerScript.transform.position);
        }
        // Update animation speed
        float agentSpeed = agent.velocity.magnitude;
        float animSpeed = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(animSpeed, agentSpeed, Time.deltaTime * animSpeedTrans));

        // Increment timers
        timeSinceLastHit += Time.deltaTime;

        if (target != null)
        {
            if (canSeeTarget())
            {
                // Adjust stopping distance for attacking
                agent.stoppingDistance = attackRange - rangeTuner;

                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                if (!agent.pathPending && distanceToTarget > attackRange + rangeTuner)
                {
                    // Resume movement if the target is out of melee range
                    agent.isStopped = false;
                    agent.SetDestination(target.transform.position);
                }
                else if (!agent.pathPending && distanceToTarget <= attackRange)
                {
                    // Stop and attack if within melee range
                    agent.isStopped = true;
                    faceTarget();

                    if (!isAttacking)
                    {

                        StartCoroutine(melee()); // Trigger attack animation
                    }
                }
            }
            else if (timeSinceLastHit >= timeToSwitchTarget)
            {
                FindClosestTarget();
            }
        }
        else
        {
            // Adjust stopping distance for roaming
            agent.stoppingDistance = 0;

            if (timeSinceLastHit >= timeToSwitchTarget)
            {
                FindClosestTarget();
            }

            if (!isRoaming && agent.remainingDistance < 0.05f)
            {
                StartCoroutine(roam());
            }
        }

        HandleAudio();
        HandleConversion();
    }



    public float CalculateDamage(float baseDamage, EffectType effectType)
    {
        float multiplier = 1f; // Default 

        // Determine resistance/weakness
        switch (effectType)
        {
            case EffectType.Fire:
                multiplier = fireResistance == 0 ? 1.5f : fireResistance == 2 ? 0.5f : 1f;
                break;

            case EffectType.Ice:
                multiplier = iceResistance == 0 ? 1.5f : iceResistance == 2 ? 0.5f : 1f;
                break;

            case EffectType.Lightning:
                multiplier = lightningResistance == 0 ? 1.5f : lightningResistance == 2 ? 0.5f : 1f;
                break;
        }

        // Apply tier scaling
        float tierMultiplier = tierMultipliers[(int)enemyTier];
        multiplier *= tierMultiplier;

        // Final damage calculation
        float finalDamage = baseDamage * multiplier;
        Debug.Log($"Effect: {effectType} | Base Damage: {baseDamage} | Multiplier: {multiplier} | Final Damage: {finalDamage}");
        return finalDamage;
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

    public void takeDamage(int amount, GameObject attacker, EffectType? effectType = null)
    {
        if (isDead) return;

        float finalDamage = amount;

        //Adjusted Damage 
        if (effectType != null)
        {
            finalDamage = CalculateDamage(amount, effectType.Value);
        }


        HP -= amount;
        anim.SetTrigger("Damage");
        popupDamage.text = amount.ToString();
        Instantiate(popupDamagePrefab, transform.position, Quaternion.identity);
        StartCoroutine(flashRed());

        dmgPoints(finalDamage, popupDamagePrefab);

        timeSinceLastHit = 0f;
        target = attacker;
        if (attacker != null)
        {
            agent.SetDestination(attacker.transform.position);
        }
        if (HP <= 0)
        {
            isDead = true;
            if (dropItem != null)
            {
                Instantiate(dropItem, this.transform.position + Vector3.up, Quaternion.identity);
            }
            StartCoroutine(Die());
            gameManager.instance.updateGameGoal(-1);

        }
    }
    void dmgPoints(float amt, GameObject popupPrefab)
    {
        GameObject popup = Instantiate(popupPrefab, transform.position, Quaternion.identity);
        popup.GetComponent<PopUpDmgTxt>().Intialize(amt);
    }
    private IEnumerator Die()
    {
        anim.SetTrigger("Death");
        agent.isStopped = true;

        yield return new WaitForSeconds(2f);
        OnDeath?.Invoke();
        Destroy(gameObject);
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
        //anim.SetTrigger("Melee");
        anim.SetTrigger("LeftAttack");
        audioSource.PlayOneShot(attackSound, attackSoundVolume);

        // Get the current animation's state information
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(1);
        float animDelay = stateInfo.length;
        hitPoint.SetActive(true);
        Debug.Log("collider one");
        yield return new WaitForSeconds(animDelay - .2f);
        hitPoint.SetActive(false);
        Debug.Log("Collider off");

        yield return new WaitForSeconds(attackRate);
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
        if (!isConverting && enemyTier != EnemyTier.Tier4)
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
        else if (enemyTier == EnemyTier.Tier4) { }
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

        friendlyAI.HP = this.HP;
        friendlyAI.faceTargetSpeed = this.faceTargetSpeed;
        friendlyAI.animSpeedTrans = this.animSpeedTrans;
        friendlyAI.attackRate = this.attackRate;
        friendlyAI.meleeDamage = this.meleeDamage;
        friendlyAI.attackRange = this.attackRange;

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

    internal void ModifySpeed(float newSpeed)
    {
        if (agent != null)
        {
            agent.speed = newSpeed;
        }
    }

    internal void ResetSpeed()
    {
        if (agent != null)
        {
            agent.speed = originalSpeed;
        }
    }

    internal void SetStunned(bool stunned)
    {
        isStunned = stunned;
        if (stunned)
        {
            ModifySpeed(0);
        }
        else
        {
            ResetSpeed();
        }
    }

    public float GetAgentSpeed()
    {
        return agent != null ? agent.speed : 0f;
    }


    public enum EnemyTier { Tier1, Tier2, Tier3, Tier4 }



}

