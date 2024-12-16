using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FriendlyExploderAI : MonoBehaviour, IDamage, IFriendly
{
    [Header("-----References-----")]
    public NavMeshAgent agent;
    public GameObject model;
    public Transform headPos;
    private GameObject player;

    [Header("-----Stats-----")]
    public int HP;
    public float faceTargetSpeed = 5f;
    public float detectionRadius = 15f;
    public float explosionRange = 2f;
    public float explosionDamageRadius = 8f;
    public int explosionDamage = 50;
    public int flashCount = 3;
    public float flashDuration = 0.2f;

    [Header("-----Roaming & Following-----")]
    public float followRadius = 5f;
    public float idleRoamRadius = 3f;
    public float minIdleRoamInterval = 2f;
    public float maxIdleRoamInterval = 5f;
    public float playerMoveThreshold = 0.2f;
    private Vector3 followOffset;
    private float lastIdleRoamTime = 0f;
    private float idleRoamInterval;
    private Vector3 lastPlayerPosition;
    private bool isRoaming = false;

    [Header("-----Teleport Settings-----")]
    public float maxTeleportDistance = 20f;
    public float noEnemyDuration = 5f;
    private float noEnemyTimer = 0f;

    //[SerializeField] private float roamTimer = 3f;
    private float lastRoamTime = 0f;

    [Header("-----Audio-----")]
    public AudioClip walkSound;
    public AudioClip explosionSound;
    [Range(0, 1)] public float walkSoundVolume = 0.5f;
    [Range(0, 1)] public float explosionSoundVolume = 1f;

    [Header("-----Audio Sources-----")]
    public AudioSource walkingAudioSource;
    public AudioSource explosionAudioSource;

    public List<Renderer> renderers = new List<Renderer>();
    public List<List<Color>> originalColors = new List<List<Color>>();

    private GameObject currentEnemy;
    private bool isExploding = false;
    private bool isDead = false;

    public delegate void OnDeathEvent();
    public event OnDeathEvent OnDeath;

    void Start()
    {
        agent = agent ?? GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");

        agent.updateRotation = false;

        if (walkingAudioSource == null)
            Debug.LogError("Walking AudioSource is not assigned in the Inspector!");
        else
        {
            walkingAudioSource.clip = walkSound;
            walkingAudioSource.loop = true;
            walkingAudioSource.volume = walkSoundVolume;
        }

        if (explosionAudioSource == null)
            Debug.LogError("Explosion AudioSource is not assigned in the Inspector!");
        else
        {
            explosionAudioSource.clip = explosionSound;
            explosionAudioSource.loop = false;
            explosionAudioSource.volume = explosionSoundVolume;
        }

        Renderer[] allRenderers = model.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(materials[i]);
            }
            renderer.materials = materials;

            renderers.Add(renderer);

            List<Color> rendererOriginalColors = new List<Color>();
            foreach (Material mat in renderer.materials)
            {
                rendererOriginalColors.Add(mat.color);
            }
            originalColors.Add(rendererOriginalColors);
        }

        ApplyGreenTint();
        gameManager.instance.RegisterFriendly(this);

        lastIdleRoamTime = Time.time + Random.Range(0f, maxIdleRoamInterval);
        idleRoamInterval = Random.Range(minIdleRoamInterval, maxIdleRoamInterval);
        lastPlayerPosition = player.transform.position;
        lastRoamTime = Time.time;
    }

    void ApplyGreenTint()
    {
        Color greenTint = Color.green;
        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer renderer = renderers[i];
            List<Color> rendererOriginalColors = originalColors[i];
            Material[] materials = renderer.materials;
            for (int m = 0; m < materials.Length; m++)
            {
                materials[m].color = greenTint;
                rendererOriginalColors[m] = greenTint;
            }
        }
    }

    void Update()
    {
        if (isDead || isExploding)
            return;

        if (player == null)
            return;

        // Check if player moved to reset roaming if needed
        if (Vector3.Distance(player.transform.position, lastPlayerPosition) > playerMoveThreshold)
        {
            isRoaming = false;
            StopAllCoroutines();
            FollowPlayer();
        }
        lastPlayerPosition = player.transform.position;

        bool hasEnemy = canSeeTarget();
        if (!hasEnemy)
        {
            currentEnemy = null;
        }

        if (hasEnemy && currentEnemy != null)
        {
            noEnemyTimer = 0f;
            agent.isStopped = false;
            agent.SetDestination(currentEnemy.transform.position);

            // Only face target when there's an enemy
            FaceTarget(currentEnemy.transform.position);

            float dist = Vector3.Distance(transform.position, currentEnemy.transform.position);
            if (dist <= explosionRange)
            {
                StartCoroutine(StartExplosionSequence());
            }
        }
        else
        {
            // No enemy found
            noEnemyTimer += Time.deltaTime;

            if (!isRoaming)
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
                // Here we do NOT call FaceDirection to avoid forced rotation towards direction
                // The Exploder keeps spinning from spin.cs without being forced to rotate towards movement
            }

            // Check distance for teleport
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distToPlayer > maxTeleportDistance && noEnemyTimer >= noEnemyDuration && !isExploding && !isRoaming)
            {
                TeleportToPlayerSide();
                noEnemyTimer = 0f;
            }
        }

        HandleAudio();
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
        agent.isStopped = false;
        agent.stoppingDistance = 0.5f;
        Vector3 followPosition = player.transform.position + followOffset;
        agent.SetDestination(followPosition);
    }

    // Same IdleRoam logic as before

    void TeleportToPlayerSide()
    {
        Vector3 teleportPosition = player.transform.position + followOffset;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(teleportPosition, out hit, 2f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            // Re-enable agent after teleport
            agent.enabled = true;
            agent.Warp(hit.position);
        }
        else
        {
            transform.position = player.transform.position;
            agent.enabled = true;
            agent.Warp(player.transform.position);
        }
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

    bool canSeeTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        GameObject bestTarget = null;
        float shortestDist = detectionRadius;

        foreach (var h in hits)
        {
            if (h.CompareTag("Enemy") || h.CompareTag("EnemySpewer") || h.CompareTag("EnemyExploder"))
            {
                float dist = Vector3.Distance(transform.position, h.transform.position);
                if (dist < shortestDist)
                {
                    Vector3 dir = h.transform.position - headPos.position;
                    if (Physics.Raycast(headPos.position, dir, out RaycastHit hit, detectionRadius))
                    {
                        if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("EnemySpewer") || hit.collider.CompareTag("EnemyExploder"))
                        {
                            bestTarget = h.gameObject;
                            shortestDist = dist;
                        }
                    }
                }
            }
        }

        if (bestTarget != null)
        {
            currentEnemy = bestTarget;
            return true;
        }

        return false;
    }

    void FaceTarget(Vector3 targetPosition)
    {
        // Only rotate towards enemy if we have one
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
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

    IEnumerator StartExplosionSequence()
    {
        if (isDead) yield break;

        isExploding = true;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        walkingAudioSource.Stop();

        for (int i = 0; i < flashCount; i++)
        {
            if (i == flashCount - 1)
            {
                model.transform.localScale *= 2f;

                foreach (Renderer renderer in renderers)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.color = Color.white;
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.white * 5f);
                    }
                }

                yield return new WaitForSeconds(flashDuration * 2f);
                Explode();
                yield break;
            }
            else
            {
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.color = Color.white;
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.white * 5f);
                    }
                }
                yield return new WaitForSeconds(flashDuration);

                for (int r = 0; r < renderers.Count; r++)
                {
                    Renderer renderer = renderers[r];
                    List<Color> rendererOriginalColors = originalColors[r];
                    for (int m = 0; m < renderer.materials.Length; m++)
                    {
                        Material mat = renderer.materials[m];
                        mat.color = rendererOriginalColors[m];
                        mat.SetColor("_EmissionColor", Color.black);
                        mat.DisableKeyword("_EMISSION");
                    }
                }
                yield return new WaitForSeconds(flashDuration);
            }
        }
    }

    bool ValidExplosionTarget(GameObject obj)
    {
        if (CompareTag("FriendlyExploder"))
        {
            return (obj.CompareTag("Enemy") || obj.CompareTag("EnemySpewer") || obj.CompareTag("EnemyExploder"));
        }
        else if (CompareTag("EnemyExploder"))
        {
            return (obj.CompareTag("Player") || obj.CompareTag("Friendly") ||
                    obj.CompareTag("FriendlySpewer") || obj.CompareTag("FriendlyExploder"));
        }
        return false;
    }

    void Explode()
    {
        walkingAudioSource.Stop();
        isDead = true;
        OnDeath?.Invoke();
        gameManager.instance.updateGameGoal(-1);

        HashSet<GameObject> damagedObjects = new HashSet<GameObject>();
        Collider[] currentHits = Physics.OverlapSphere(transform.position, explosionDamageRadius);
        foreach (var c in currentHits)
        {
            GameObject obj = c.gameObject; // Changed from c.transform.root.gameObject to c.gameObject

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= explosionDamageRadius && ValidExplosionTarget(obj))
            {
                IDamage damageable = obj.GetComponent<IDamage>();
                if (damageable != null && !damagedObjects.Contains(obj))
                {
                    damageable.takeDamage(explosionDamage, gameObject);
                    damagedObjects.Add(obj);
                }
            }
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionSoundVolume);
        }

        Destroy(gameObject);
    }

    public void takeDamage(int amount, GameObject attacker, EffectType? effectType = null)
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
            foreach (Material mat in renderer.materials)
            {
                mat.color = Color.red;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EMISSIONColor", Color.red * 5f);
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (int r = 0; r < renderers.Count; r++)
        {
            Renderer renderer = renderers[r];
            List<Color> rendererOriginalColors = originalColors[r];
            for (int m = 0; m < renderer.materials.Length; m++)
            {
                Material mat = renderer.materials[m];
                mat.color = rendererOriginalColors[m];
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
            }
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Destroy(gameObject);
    }

    void HandleAudio()
    {
        if (isDead || isExploding) return;

        if (agent.enabled && !agent.isStopped && agent.velocity.magnitude > 0.1f && !walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Play();
        }
        else if ((agent.isStopped || agent.velocity.magnitude <= 0.1f) && walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Stop();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionDamageRadius);
    }
}