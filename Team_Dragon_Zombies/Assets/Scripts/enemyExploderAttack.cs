using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyExploderAttack : MonoBehaviour, IDamage
{
    [Header("-----References-----")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject model;
    [SerializeField] private Transform headPos;
    private GameObject player;

    [Header("-----Stats-----")]
    public int HP;
    [SerializeField] private float faceTargetSpeed = 5f;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] public float explosionRange = 2f;         // Trigger range
    [SerializeField] public float explosionDamageRadius = 5f; // Damage radius
    [SerializeField] public int explosionDamage = 50;
    [SerializeField] public int flashCount = 3;
    [SerializeField] public float flashDuration = 0.2f;

    [Header("-----Roaming-----")]
    [SerializeField] private float roamTimer = 3f;
    [SerializeField] private float roamDist = 10f; // Radius around startingPos for roaming
    private float lastRoamTime = 0f;
    private float spinSpeed = 100f;
    private bool isRoaming = false;
    private Vector3 startingPos;

    [Header("-----Audio-----")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField][Range(0, 1)] private float walkSoundVolume = 0.5f;
    [SerializeField][Range(0, 1)] private float explosionSoundVolume = 1f;
    [SerializeField] private AudioSource walkingAudioSource;
    [SerializeField] private AudioSource explosionAudioSource;

    [Header("-----Enemy Conversion-----")]
    public EnemyTier enemyTier;
    [SerializeField] private float conversionTime;
    private float conversionTimer = 0f;
    private bool isConverting = false;

    private GameObject target;
    private bool isExploding = false;
    private bool isDead = false;

    public List<Renderer> renderers = new List<Renderer>();
    public List<List<Color>> originalColors = new List<List<Color>>();
    public Color flashColor = Color.white;

    public delegate void OnDeathEvent();
    public event OnDeathEvent OnDeath;

    void Start()
    {
        gameObject.tag = "EnemyExploder";
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false; // We control rotation by code (spin), not agent.
        player = GameObject.FindWithTag("Player");
        startingPos = transform.position;

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

        if (walkingAudioSource == null)
            Debug.LogError("Walking AudioSource is not assigned in the Inspector!");
        if (explosionAudioSource == null)
            Debug.LogError("Explosion AudioSource is not assigned in the Inspector!");

        gameManager.instance.updateGameGoal(1);
        lastRoamTime = Time.time;
    }

    void Update()
    {
        if (isDead) return;

        if (!isExploding && !isConverting)
        {
            if (!canSeeTarget()) // Check line-of-sight: if can't see, no target
                target = null;

            if (target != null)
            {
                // We have a visible target within detection radius
                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist <= explosionRange)
                {
                    StartCoroutine(StartExplosionSequence());
                }
                else
                {
                    // Move towards target
                    agent.isStopped = false;
                    agent.SetDestination(target.transform.position);
                    FaceTarget(target.transform.position);
                }
            }
            else
            {
                // No target visible: Roam and spin
                RoamSpinLogic();
            }

            HandleConversion();
        }

        HandleAudio();
    }

    void RoamSpinLogic()
    {
        // Always spin
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

        // If not currently moving anywhere or time to pick new roam point
        if (!isRoaming && Time.time - lastRoamTime > roamTimer)
        {
            StartCoroutine(Roam());
        }
    }

    IEnumerator Roam()
    {
        isRoaming = true;
        lastRoamTime = Time.time;

        // Pick a random point around startingPos
        Vector2 randomCircle = Random.insideUnitCircle * roamDist;
        Vector3 roamPos = startingPos + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(roamPos, out hit, roamDist, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        // Wait until we reach the roam point or a short time passes
        float startWait = Time.time;
        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
        {
            if (Time.time - startWait > roamTimer) // Fail-safe if stuck
                break;
            yield return null;
        }

        isRoaming = false;
    }

    void FollowPlayer()
    {
        if (player != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);

            if (agent.velocity.magnitude > 0.1f)
            {
                FaceDirection(agent.velocity.normalized);
            }
            else
            {
                FaceTarget(player.transform.position);
            }
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
                isConverting = false;
            }
        }
    }

    bool canSeeTarget()
    {
        // Attempt to find a target (player/friendlies) within detection radius
        // Since 360 vision, no angle check. Just check line-of-sight.
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        GameObject bestTarget = null;
        float shortestDist = detectionRadius;

        foreach (var h in hits)
        {
            if (h.CompareTag("Player") || h.CompareTag("Friendly") || h.CompareTag("FriendlySpewer"))
            {
                float dist = Vector3.Distance(transform.position, h.transform.position);
                if (dist < shortestDist)
                {
                    // Check line-of-sight
                    Vector3 dir = h.transform.position - headPos.position;
                    if (Physics.Raycast(headPos.position, dir, out RaycastHit hit, detectionRadius))
                    {
                        if ((hit.collider.CompareTag("Player") ||
                             hit.collider.CompareTag("Friendly") ||
                             hit.collider.CompareTag("FriendlySpewer")))
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
            target = bestTarget;
            return true;
        }

        return false;
    }

    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        }
    }

    IEnumerator StartExplosionSequence()
    {
        if (isConverting || isDead) yield break;

        isExploding = true;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        // Explode after flashes
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
        if (CompareTag("EnemyExploder"))
        {
            return (obj.CompareTag("Player") || obj.CompareTag("Friendly") ||
                    obj.CompareTag("FriendlySpewer") || obj.CompareTag("FriendlyExploder"));
        }
        else if (CompareTag("FriendlyExploder"))
        {
            return (obj.CompareTag("Enemy") || obj.CompareTag("EnemySpewer") || obj.CompareTag("EnemyExploder"));
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
            GameObject obj = c.transform.root.gameObject;
            if (ValidExplosionTarget(obj))
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
            foreach (Material mat in renderer.materials)
            {
                mat.color = Color.red;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.red * 5f);
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
        gameObject.tag = "FriendlyExploder";

        int friendlyLayer = LayerMask.NameToLayer("FriendlyExploder");
        if (friendlyLayer != -1)
        {
            gameObject.layer = friendlyLayer;
        }

        FriendlyExploderAI friendlyExploder = gameObject.AddComponent<FriendlyExploderAI>();

        friendlyExploder.HP = this.HP;
        friendlyExploder.faceTargetSpeed = this.faceTargetSpeed;
        friendlyExploder.detectionRadius = this.detectionRadius;
        friendlyExploder.explosionRange = this.explosionRange;
        friendlyExploder.explosionDamage = this.explosionDamage;
        friendlyExploder.flashCount = this.flashCount;
        friendlyExploder.flashDuration = this.flashDuration;

        friendlyExploder.explosionDamageRadius = this.explosionDamageRadius;

        friendlyExploder.agent = this.agent;
        friendlyExploder.model = this.model;
        friendlyExploder.headPos = this.headPos;
        friendlyExploder.walkSound = this.walkSound;
        friendlyExploder.explosionSound = this.explosionSound;
        friendlyExploder.walkSoundVolume = this.walkSoundVolume;
        friendlyExploder.explosionSoundVolume = this.explosionSoundVolume;

        friendlyExploder.walkingAudioSource = this.walkingAudioSource;
        friendlyExploder.explosionAudioSource = this.explosionAudioSource;

        friendlyExploder.renderers = this.renderers;
        friendlyExploder.originalColors = this.originalColors;

        gameManager.instance.updateGameGoal(-1);

        Destroy(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionDamageRadius);
    }
}
