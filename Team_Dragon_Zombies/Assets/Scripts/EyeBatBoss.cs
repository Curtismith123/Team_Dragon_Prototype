using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

[System.Serializable]
public class EnemySpawnInfo
{
    [Tooltip("Enemy prefab to spawn.")]
    public GameObject enemyPrefab;

    [Tooltip("Time interval between spawns for this enemy type.")]
    public float spawnInterval = 4f;
}

public class EyeBatBoss : MonoBehaviour, IDamage
{
    [Header("----- References -----")]
    [SerializeField] private Transform ShootPos;
    [SerializeField] private Transform[] extraShootPositions;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private GameObject[] platforms;
    [SerializeField] private GameObject lava;
    [SerializeField] private GameObject player;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] public GameObject dropItem;
    [SerializeField] public Transform dropLocation;

    [Header("----- Audio -----")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip boulderFallingClip;
    [SerializeField] private AudioClip phaseTransitionClipPhase1to2;
    [SerializeField] private AudioClip phaseTransitionClipPhase2to3;
    [SerializeField] private AudioClip firingClip;
    [SerializeField] private AudioClip dyingClip;
    [SerializeField] private float boulderVolume = 1f;
    [SerializeField] private float phaseTransitionVolume1to2 = 1f;
    [SerializeField] private float phaseTransitionVolume2to3 = 1f;
    [Range(0f, 1f)][SerializeField] private float firingVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float dyingVolume = 1f;

    [Header("----- Stats -----")]
    [SerializeField] private int maxHP = 100;
    private int currentHP;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private float turnSpeed = 2f;

    [Header("----- Phase Firing Settings -----")]
    [SerializeField] private int bulletsPerSet = 5;
    [SerializeField] private float timeBetweenBulletsInSet = 0.2f;
    [SerializeField] private float timeBetweenSets = 2f;
    [SerializeField] private float extraShootPosFireRate = 1.0f;
    [SerializeField] private float horizontalOffsetDistance = 2f;

    [Header("----- Phase Timings and Spawns -----")]
    //[SerializeField] private float enemySpawnInterval = 4f;
    [SerializeField] private float boulderSpawnInterval = 2f;

    [Header("----- Spawn Settings -----")]
    [SerializeField] private float boulderSpawnRadius = 5f;
    [SerializeField] private float enemySpawnRadius = 5f;
    [SerializeField] private float playerFOVAngle = 90f;

    [Header("----- Lava and Platform Movement -----")]
    [Header("These times swap at the end of phase 2")]
    [SerializeField] private float lavaRiseTime = 3f;
    [SerializeField] private float platformRiseTime = 1.5f;

    [Tooltip("Additional height above the player's foot level for lava during Phase 2.")]
    [SerializeField] private float lavaHeightOffset = 2f;

    [Header("----- Floor Detection -----")]
    [SerializeField] private float sinkSpeed = 1f;
    [SerializeField] private GameObject floorCheckObj;

    [Header("----- Emission Settings -----")]
    [SerializeField] private Color deathEmissionColor = Color.white;
    [SerializeField] private float deathEmissionIntensity = 5f;

    [SerializeField] private Color flashEmissionColor = Color.red;
    [SerializeField] private float flashEmissionIntensity = 2f;

    [SerializeField] private Color hitFlashEmissionColor = Color.white;
    [SerializeField] private float hitFlashEmissionIntensity = 3f;

    [Header("----- Boulder Spawn Heights -----")]
    [Tooltip("Minimum height above the player from which boulders can spawn.")]
    [SerializeField] private float boulderMinSpawnHeight = 15f;

    [Tooltip("Maximum height above the player from which boulders can spawn.")]
    [SerializeField] private float boulderMaxSpawnHeight = 25f;

    [Header("----- Enemy Spawn Settings -----")]
    [Tooltip("Array of enemy types the boss can spawn.")]
    [SerializeField] private EnemySpawnInfo[] enemySpawnInfos;

    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;
    private BossPhase previousPhase = BossPhase.Phase1;
    private bool isDead = false;
    private Color originalColor;
    private Coroutine flashRoutine;
    private Coroutine firingRoutine;
    private Coroutine extraFiringRoutine;
    private Coroutine hitFlashRoutine;
    private float lastEnemyTime;
    private float lastBoulderTime;
    private List<Renderer> allRenderers = new List<Renderer>();
    private Vector3 lavaOriginalPos;
    private Vector3[] platformsOriginalPos;
    private bool canEngage = false;

    private List<Coroutine> enemySpawnCoroutines = new List<Coroutine>();

    void Start()
    {
        currentHP = maxHP;
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            allRenderers.Add(rend);
        }
        if (allRenderers.Count > 0)
            originalColor = allRenderers[0].material.color;
        else
            originalColor = Color.white;

        gameManager.instance.updateGameGoal(1);

        if (lava != null)
            lavaOriginalPos = lava.transform.position;
        if (platforms != null && platforms.Length > 0)
        {
            platformsOriginalPos = new Vector3[platforms.Length];
            for (int i = 0; i < platforms.Length; i++)
                if (platforms[i] != null)
                    platformsOriginalPos[i] = platforms[i].transform.position;
        }

        currentPhase = BossPhase.Phase1;
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;
        if (!canEngage)
        {
            if (HasLineOfSightToPlayer())
            {
                canEngage = true;
                StartFiringPatternForCurrentPhase();
            }
        }
        if (!canEngage) return;
        UpdatePhase();
        RotateToFacePlayer();
        if (currentPhase == BossPhase.Phase3)
            HandlePhase3Actions();
    }

    bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;
        Vector3 dirToPlayer = player.transform.position - transform.position;
        float dist = dirToPlayer.magnitude;
        Ray ray = new Ray(transform.position + Vector3.up, dirToPlayer.normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, dist))
        {
            if (hit.collider.gameObject == player)
                return true;
        }
        return false;
    }

    void UpdatePhase()
    {
        float hpPercent = (float)currentHP / maxHP;
        BossPhase newPhase;
        if (hpPercent > 0.667f)
            newPhase = BossPhase.Phase1;
        else if (hpPercent > 0.33f)
            newPhase = BossPhase.Phase2;
        else
            newPhase = BossPhase.Phase3;

        if (newPhase != currentPhase)
        {
            previousPhase = currentPhase;
            currentPhase = newPhase;
            PhaseChanged();
        }
    }

    void PhaseChanged()
    {

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        if (firingRoutine != null) StopCoroutine(firingRoutine);
        if (extraFiringRoutine != null) StopCoroutine(extraFiringRoutine);
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);

        StopAllEnemySpawnCoroutines();

        if (previousPhase == BossPhase.Phase1 && currentPhase == BossPhase.Phase2)
            audioSource.PlayOneShot(phaseTransitionClipPhase1to2, phaseTransitionVolume1to2);
        else if (previousPhase == BossPhase.Phase2 && currentPhase == BossPhase.Phase3)
            audioSource.PlayOneShot(phaseTransitionClipPhase2to3, phaseTransitionVolume2to3);

        if (currentPhase == BossPhase.Phase2)
        {
            flashRoutine = StartCoroutine(FlashRedRoutine(1.0f));
            RaiseLavaAndPlatforms();
            if (warningText != null)
            {
                warningText.gameObject.SetActive(true);
                StartCoroutine(HideWarningTextAfterDelay(3f));
            }
        }
        else if (currentPhase == BossPhase.Phase3)
        {
            flashRoutine = StartCoroutine(FlashRedRoutine(0.5f));
            LowerLavaAndPlatforms();

            StartAllEnemySpawnCoroutines();
        }
        else
        {
            ResetColor();
            LowerLavaAndPlatforms();
        }

        StartFiringPatternForCurrentPhase();
    }

    void ResetColor()
    {
        foreach (Renderer rend in allRenderers)
        {
            rend.material.color = originalColor;
            rend.material.SetColor("_EmissionColor", originalColor * 1f);
        }
    }

    void StartFiringPatternForCurrentPhase()
    {
        if (!canEngage) return;
        switch (currentPhase)
        {
            case BossPhase.Phase1:
            case BossPhase.Phase2:
                firingRoutine = StartCoroutine(Phase1FiringPattern());
                break;
            case BossPhase.Phase3:
                firingRoutine = StartCoroutine(Phase1FiringPattern());
                extraFiringRoutine = StartCoroutine(Phase3ExtraPositionsFiring());
                break;
        }
    }

    IEnumerator Phase1FiringPattern()
    {
        while (!isDead && (currentPhase == BossPhase.Phase1 || currentPhase == BossPhase.Phase2 || currentPhase == BossPhase.Phase3))
        {
            for (int i = 0; i < bulletsPerSet; i++)
            {
                audioSource.PlayOneShot(firingClip, firingVolume);
                FireBullet(ShootPos, Vector3.zero);
                yield return new WaitForSeconds(timeBetweenBulletsInSet);
            }
            yield return new WaitForSeconds(timeBetweenSets);
        }
    }

    IEnumerator Phase3ExtraPositionsFiring()
    {
        bool fireFromLeft = true;
        while (!isDead && currentPhase == BossPhase.Phase3)
        {
            Vector3 rightDir = transform.right;
            Vector3 leftOffset = -rightDir * horizontalOffsetDistance;
            Vector3 rightOffset = rightDir * horizontalOffsetDistance;
            if (extraShootPositions.Length > 0 && fireFromLeft)
            {
                audioSource.PlayOneShot(firingClip, firingVolume);
                FireBullet(extraShootPositions[0], leftOffset);
            }
            else if (extraShootPositions.Length > 1 && !fireFromLeft)
            {
                audioSource.PlayOneShot(firingClip, firingVolume);
                FireBullet(extraShootPositions[1], rightOffset);
            }
            fireFromLeft = !fireFromLeft;
            yield return new WaitForSeconds(extraShootPosFireRate);
        }
    }

    void FireBullet(Transform shootPos, Vector3 playerOffset)
    {
        if (bulletPrefab == null || shootPos == null || player == null) return;
        Vector3 playerPos = player.transform.position + playerOffset;
        Vector3 direction = (playerPos - shootPos.position).normalized;
        GameObject projectile = Instantiate(bulletPrefab, shootPos.position, Quaternion.LookRotation(direction));
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
        if (projectile.TryGetComponent(out Bullet bullet))
        {
            bullet.SetSpeed(projectileSpeed);
            bullet.SetDamage(projectileDamage);
            bullet.SetAttacker(gameObject);
            bullet.SetDestroyTime(5f);
        }
    }

    void RaiseLavaAndPlatforms()
    {
        if (player == null) return;
        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider == null) return;
        float playerFootHeight = playerCollider.bounds.min.y;

        float lavaTargetY = playerFootHeight + lavaHeightOffset;

        if (lava != null)
            StartCoroutine(MoveObjectUpwards(lava.transform, lava.transform.position.y, lavaTargetY, lavaRiseTime));

        if (platforms != null)
        {
            for (int i = 0; i < platforms.Length; i++)
                if (platforms[i] != null)
                    StartCoroutine(MoveObjectUpwards(platforms[i].transform, platforms[i].transform.position.y, playerFootHeight, platformRiseTime));
        }
    }

    void LowerLavaAndPlatforms()
    {
        if (lava != null)
            StartCoroutine(MoveObjectUpwards(lava.transform, lava.transform.position.y, lavaOriginalPos.y, lavaRiseTime));
        if (platforms != null)
        {
            for (int i = 0; i < platforms.Length; i++)
                if (platforms[i] != null)
                    StartCoroutine(MoveObjectUpwards(platforms[i].transform, platforms[i].transform.position.y, platformsOriginalPos[i].y, lavaRiseTime));
        }
    }

    IEnumerator MoveObjectUpwards(Transform obj, float startY, float targetY, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY, targetY, elapsed / duration);
            obj.position = new Vector3(obj.position.x, newY, obj.position.z);
            yield return null;
        }
        obj.position = new Vector3(obj.position.x, targetY, obj.position.z);
    }

    void HandlePhase3Actions()
    {

        if (Time.time - lastBoulderTime >= boulderSpawnInterval)
        {
            DropBoulder();
            lastBoulderTime = Time.time;
        }
    }

    void StartAllEnemySpawnCoroutines()
    {
        foreach (EnemySpawnInfo enemyInfo in enemySpawnInfos)
        {
            if (enemyInfo.enemyPrefab != null)
            {
                Coroutine spawnCoroutine = StartCoroutine(SpawnEnemyRoutine(enemyInfo));
                enemySpawnCoroutines.Add(spawnCoroutine);
            }
        }
    }

    void StopAllEnemySpawnCoroutines()
    {
        foreach (Coroutine coroutine in enemySpawnCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        enemySpawnCoroutines.Clear();
    }

    IEnumerator SpawnEnemyRoutine(EnemySpawnInfo enemyInfo)
    {
        while (!isDead && currentPhase == BossPhase.Phase3)
        {
            SpawnEnemyOutsidePlayerFOV(enemyInfo.enemyPrefab);
            yield return new WaitForSeconds(enemyInfo.spawnInterval);
        }
    }

    void SpawnEnemyOutsidePlayerFOV(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || player == null) return;
        int attempts = 15;
        Vector3 spawnPos = player.transform.position;
        NavMeshHit hit;
        float sampleRadius = enemySpawnRadius * 2f;

        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * enemySpawnRadius;
            spawnPos = player.transform.position + new Vector3(randomOffset.x, 0, randomOffset.z);

            if (!IsWithinPlayerFOV(spawnPos))
            {
                if (NavMesh.SamplePosition(spawnPos, out hit, sampleRadius, NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                    break;
                }
            }
        }

        if (NavMesh.SamplePosition(spawnPos, out hit, sampleRadius, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
                enemy.transform.position = hit.position;
                agent.Warp(hit.position);
                agent.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning("EyeBatBoss: Failed to find a valid spawn position on the NavMesh for enemy.");
        }
    }

    bool IsWithinPlayerFOV(Vector3 position)
    {
        if (player == null) return false;
        Vector3 toTarget = (position - player.transform.position).normalized;
        float angle = Vector3.Angle(player.transform.forward, toTarget);
        float halfFOV = playerFOVAngle / 2f;
        return (angle <= halfFOV);
    }

    void DropBoulder()
    {
        if (boulderPrefab == null || player == null)
        {
            Debug.LogWarning($"{name}: Unable to spawn boulder. BoulderPrefab or Player is null.");
            return;
        }
        Quaternion randomRotation = Random.rotation;
        float rand = Random.value;
        Vector3 spawnPos;
        float spawnHeight;

        if (rand < 0.33f)
        {
            spawnHeight = Random.Range(boulderMinSpawnHeight, boulderMaxSpawnHeight);
            spawnPos = player.transform.position + new Vector3(0, spawnHeight, 0);
        }
        else
        {
            Vector3 randomOffset = Random.insideUnitSphere * boulderSpawnRadius;
            randomOffset.y = 0f;
            spawnHeight = Random.Range(boulderMinSpawnHeight, boulderMaxSpawnHeight);
            spawnPos = player.transform.position + randomOffset + Vector3.up * spawnHeight;
        }

        Instantiate(boulderPrefab, spawnPos, randomRotation);
        AudioSource.PlayClipAtPoint(boulderFallingClip, spawnPos, boulderVolume);
    }

    IEnumerator FlashRedRoutine(float interval)
    {
        while (!isDead && (currentPhase == BossPhase.Phase2 || currentPhase == BossPhase.Phase3))
        {

            SetAllRenderersColor(flashEmissionColor, flashEmissionIntensity);
            yield return new WaitForSeconds(0.1f);

            SetAllRenderersColor(originalColor, 1f);
            yield return new WaitForSeconds(interval);
        }
        SetAllRenderersColor(originalColor, 1f);
    }

    void SetAllRenderersColor(Color color, float emissionIntensity = 1f)
    {
        foreach (Renderer rend in allRenderers)
        {
            rend.material.color = color;
            rend.material.EnableKeyword("_EMISSION");
            Color emissionColor = color * Mathf.LinearToGammaSpace(emissionIntensity);
            rend.material.SetColor("_EmissionColor", emissionColor);
        }
    }

    void RotateToFacePlayer()
    {
        if (player == null) return;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    public void takeDamage(int amount, GameObject attacker, EffectType? effectType = null)
    {
        if (isDead) return;
        currentHP -= amount;

        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(FlashOnHit());
        if (currentHP <= 0)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            Die();
        }
    }

    IEnumerator FlashOnHit()
    {
        SetAllRenderersColor(hitFlashEmissionColor, hitFlashEmissionIntensity);
        yield return new WaitForSeconds(0.1f);
        SetAllRenderersColor(originalColor, 1f);
    }

    void Die()
    {
        isDead = true;

        if (firingRoutine != null) StopCoroutine(firingRoutine);
        if (extraFiringRoutine != null) StopCoroutine(extraFiringRoutine);
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);

        StopAllEnemySpawnCoroutines();

        SetAllRenderersColor(deathEmissionColor, deathEmissionIntensity);

        audioSource.PlayOneShot(dyingClip, dyingVolume);

        gameManager.instance.updateGameGoal(-1);
        Instantiate(dropItem, dropLocation.position, Quaternion.identity);
        StartCoroutine(SinkDown());
    }

    IEnumerator SinkDown()
    {
        SetAllRenderersColor(deathEmissionColor, deathEmissionIntensity);

        while (true)
        {
            transform.position += Vector3.down * Time.deltaTime * sinkSpeed;
            yield return null;
        }
    }

    public void DestroyBossNow()
    {
        Destroy(gameObject);
    }

    IEnumerator HideWarningTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }
}
