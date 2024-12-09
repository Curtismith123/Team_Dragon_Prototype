using System.Collections;
using UnityEngine;

public class FixedTurret : MonoBehaviour
{
    [Header("-----References-----")]
    [Tooltip("The Transform from which projectiles will be fired.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("The target the turret will aim at.")]
    [SerializeField] private Transform target;

    [Header("-----Turret Settings-----")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private int projectileDamage = 25;

    [Header("-----Projectile Settings-----")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("-----Audio-----")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField][Range(0, 1)] private float attackSoundVolume = 1f;

    [Header("-----Visual Effects-----")]
    [SerializeField] private Color turretColor = Color.blue;
    [SerializeField] private Color emissionColor = Color.blue;
    [SerializeField] private float emissionIntensity = 1f;

    private float timeSinceLastShot = 0f;
    private AudioSource audioSource;
    private Renderer[] turretRenderers;

    void Start()
    {
        if (firePoint == null)
        {
            firePoint = this.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        turretRenderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in turretRenderers)
        {
            renderer.material.color = turretColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }
    }

    void Update()
    {
        if (target == null)
            return;

        timeSinceLastShot += Time.deltaTime;

        if (timeSinceLastShot >= fireRate)
        {
            Shoot();
            timeSinceLastShot = 0f;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || target == null)
            return;

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, attackSoundVolume);
        }

        Vector3 directionToTarget = (target.position - firePoint.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(directionToTarget));

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = directionToTarget * projectileSpeed;
        }

        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(projectileDamage);
            bullet.SetSpeed(projectileSpeed);
            bullet.SetAttacker(this.gameObject);
            bullet.SetDestroyTime(5f);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
