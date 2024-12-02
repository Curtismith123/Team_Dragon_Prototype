using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.ConversionUtility;

public class enemySpewerAttack : MonoBehaviour, IDamage
{
    [Header("-----References-----")]
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPos;
    [SerializeField] private Transform SpewerPos;

    [Header("-----Stats-----")]
    [SerializeField] private int HP;
    [SerializeField] private float faceTargetSpeed;
    [SerializeField] private float viewAngle = 360f;
    [SerializeField] private float detectionRadius = 100f;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float attackRange = 100f;
    [Header("-----Projectile Settings-----")]
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public float projectileSpeed = 10f;
    private float timeSinceLastAttack = 0f;

    [Header("-----Enemy Conversion-----")]
    [SerializeField] EnemyTier enemyTier;
    [SerializeField] float conversionTime;
    private bool isConverting = false;
    private float conversionTimer = 0f;

    [Header("-----Audio-----")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField][Range(0, 1)] private float attackSoundVolume = 1f;

    private AudioSource audioSource;
    private bool isAttacking;
    private bool isDead = false;
    private GameObject target;

    private List<Renderer> renderers = new List<Renderer>();
    private Color colorOrig;
    private Vector3 targetDir;

    public delegate void OnDeathEvent();
    public event OnDeathEvent OnDeath;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (SpewerPos == null)
        {
            Debug.LogError($"{name}: SpewerPos is not assigned.");
        }
        if (projectilePrefab == null)
        {
            Debug.LogError($"{name}: ProjectilePrefab is not assigned.");
        }

        renderers.Clear();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderers.Add(renderer);
            colorOrig = renderer.material.color;
        }

        gameManager.instance.updateGameGoal(1);
    }

    void Update()
    {
        timeSinceLastAttack += Time.deltaTime;

        if (target != null && canSeeTarget())
        {
            faceTarget();

            if (!isAttacking && Vector3.Distance(transform.position, target.transform.position) <= attackRange
                && timeSinceLastAttack >= attackRate)
            {
                StartCoroutine(Shoot());
                timeSinceLastAttack = 0f;
            }
        }
        else
        {
            FindTarget();
        }
    }

    bool canSeeTarget()
    {
        if (target == null)
            return false;

        targetDir = target.transform.position - headPos.position;
        float angleToTarget = Vector3.Angle(targetDir, transform.forward);

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

    void FindTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float shortestDistance = detectionRadius;
        GameObject nearestTarget = null;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player") ||
                collider.CompareTag("Friendly") ||
                collider.CompareTag("FriendlySpewer"))
            {
                float distanceToTarget = Vector3.Distance(transform.position, collider.transform.position);
                if (distanceToTarget < shortestDistance)
                {
                    shortestDistance = distanceToTarget;
                    nearestTarget = collider.gameObject;
                }
            }
        }

        target = nearestTarget;
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

    IEnumerator Shoot()
    {
        isAttacking = true;

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, attackSoundVolume);
        }

        yield return new WaitForSeconds(0.1f);

        if (projectilePrefab == null)
        {
            Debug.LogError($"{name}: No projectile added.");
            yield break;
        }

        if (SpewerPos == null)
        {
            Debug.LogError($"{name}: SpewerPos not added.");
            yield break;
        }

        if (target == null)
        {
            yield break;
        }

        Collider[] colliders = target.GetComponents<Collider>();
        Vector3 targetCenter = Vector3.zero;
        bool validColliderFound = false;

        foreach (var collider in colliders)
        {
            if (!collider.isTrigger)
            {
                targetCenter = collider.bounds.center;
                validColliderFound = true;
                break;
            }
        }

        if (!validColliderFound)
        {
            Debug.LogError($"{name}: Target doesn't have a collider that can be hit.");
            yield break;
        }

        Vector3 directionToTarget = (targetCenter - SpewerPos.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, SpewerPos.position, Quaternion.LookRotation(directionToTarget));

        if (projectile != null)
        {
            projectile.layer = gameObject.CompareTag("FriendlySpewer")
                ? LayerMask.NameToLayer("PlayerBullet")
                : LayerMask.NameToLayer("EnemySpewerBullet");

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = directionToTarget * projectileSpeed;
            }
            else
            {
                Debug.LogError($"{name}: Projectile needs a rigidbody.");
            }

            Bullet bullet = projectile.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetSpeed(projectileSpeed);
                bullet.SetDamage(projectileDamage);
                bullet.SetAttacker(gameObject);
                bullet.SetDestroyTime(5f);
            }
            else
            {
                Debug.LogError($"{name}: Projectile needs a script on it.");
            }
        }
        else
        {
            Debug.LogError($"{name}: Can't shoot.");
        }

        yield return new WaitForSeconds(attackRate - 0.1f);
        isAttacking = false;
    }

    public void takeDamage(int amount, GameObject attacker)
    {
        if (isDead) return;

        HP -= amount;
        StartCoroutine(FlashRed());

        if (HP <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
            gameManager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
    }

    IEnumerator FlashRed()
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
        else if (enemyTier == EnemyTier.Tier4);
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
        gameObject.tag = "FriendlySpewer";

        int friendlyLayer = LayerMask.NameToLayer("FriendlySpewer");
        if (friendlyLayer != -1)
        {
            gameObject.layer = friendlyLayer;
        }

        FriendlySpewerAI friendlyAI = gameObject.AddComponent<FriendlySpewerAI>();

        friendlyAI.HP = this.HP;
        friendlyAI.faceTargetSpeed = this.faceTargetSpeed;
        friendlyAI.attackRate = this.attackRate;
        friendlyAI.attackRange = this.attackRange;
        friendlyAI.projectilePrefab = this.projectilePrefab;
        friendlyAI.projectileSpeed = this.projectileSpeed;

        friendlyAI.model = this.model;
        friendlyAI.headPos = this.headPos;
        friendlyAI.SpewerPos = this.SpewerPos;
        friendlyAI.attackSound = this.attackSound;
        friendlyAI.attackSoundVolume = this.attackSoundVolume;

        gameManager.instance.updateGameGoal(-1);

        Destroy(this);
    }
}
