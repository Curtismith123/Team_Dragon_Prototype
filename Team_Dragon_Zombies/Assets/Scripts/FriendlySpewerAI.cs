using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlySpewerAI : MonoBehaviour, IDamage, IFriendly
{
    [Header("-----References-----")]
    public Renderer model;
    public Transform headPos;
    public Transform SpewerPos;

    [Header("-----Stats-----")]
    public int HP;
    public float faceTargetSpeed;
    public float detectionRadius = 100f;
    public float attackRate = 1.5f;
    public float attackRange = 100f;
    [SerializeField] float viewAngle = 360f;
    [Header("-----Projectile Settings-----")]
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public float projectileSpeed = 10f;
    private float timeSinceLastAttack = 0f;

    [Header("-----Audio-----")]
    public AudioClip attackSound;
    [Range(0, 1)] public float attackSoundVolume = 1f;

    private AudioSource audioSource;
    private GameObject currentEnemy;

    private bool isAttacking;
    private bool isDead = false;

    private List<Color> originalColors = new List<Color>();
    private List<Renderer> renderers = new List<Renderer>();

    void Start()
    {
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
        timeSinceLastAttack += Time.deltaTime;

        if (currentEnemy != null)
        {
            if (canSeeEnemy())
            {
                faceTarget();

                if (!isAttacking && Vector3.Distance(transform.position, currentEnemy.transform.position) <= attackRange
                    && timeSinceLastAttack >= attackRate)
                {
                    StartCoroutine(Shoot());
                    timeSinceLastAttack = 0f;
                }
            }
            else
            {
                currentEnemy = null;
            }
        }
        else
        {
            FindTarget();
        }
    }

    bool canSeeEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float shortestDistance = detectionRadius;
        GameObject nearestEnemy = null;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("EnemySpewer"))
            {
                GameObject enemy = collider.gameObject;
                Vector3 directionToEnemy = enemy.transform.position - headPos.position;
                float distanceToEnemy = directionToEnemy.magnitude;

                float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy.normalized);
                if (angleToEnemy <= viewAngle)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(headPos.position, directionToEnemy.normalized, out hit, detectionRadius))
                    {
                        if (hit.collider.gameObject == enemy)
                        {
                            if (distanceToEnemy < shortestDistance)
                            {
                                shortestDistance = distanceToEnemy;
                                nearestEnemy = enemy;
                            }
                        }
                    }
                }
            }
        }

        currentEnemy = nearestEnemy;
        return currentEnemy != null;
    }

    void FindTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float shortestDistance = detectionRadius;
        GameObject nearestTarget = null;

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("EnemySpewer"))
            {
                float distanceToTarget = Vector3.Distance(transform.position, collider.transform.position);
                if (distanceToTarget < shortestDistance)
                {
                    shortestDistance = distanceToTarget;
                    nearestTarget = collider.gameObject;
                }
            }
        }

        currentEnemy = nearestTarget;
    }

    void faceTarget()
    {
        if (currentEnemy != null)
        {
            Vector3 targetDir = currentEnemy.transform.position - transform.position;
            Vector3 direction = new Vector3(targetDir.x, 0, targetDir.z);
            if (direction != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
            }
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
            Debug.LogError($"{name}: ProjectilePrefab is not assigned.");
            yield break;
        }

        if (SpewerPos == null)
        {
            Debug.LogError($"{name}: SpewerPos is not assigned.");
            yield break;
        }

        if (currentEnemy == null)
        {
            Debug.LogError($"{name}: No target to shoot at.");
            yield break;
        }

        Collider[] colliders = currentEnemy.GetComponents<Collider>();
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
            Debug.LogError($"{name}: Target does not have a valid non-trigger collider.");
            yield break;
        }

        Vector3 directionToTarget = (targetCenter - SpewerPos.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, SpewerPos.position, Quaternion.LookRotation(directionToTarget));

        if (projectile != null)
        {
            Debug.Log($"{name}: Projectile instantiated successfully at {SpewerPos.position}");
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
                Debug.LogError($"{name}: Projectile does not have a Rigidbody component.");
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
                Debug.LogError($"{name}: Bullet script is not attached to the projectile prefab.");
            }
        }
        else
        {
            Debug.LogError($"{name}: Failed to instantiate projectile.");
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
