using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Scriptable Objects/StatusEffectSO")]
public class StatusEffectSO : ScriptableObject
{
    [Header("Effect Properties")]
    public EffectType effectType;
    public AudioClip soundEffect;
    public GameObject particleEffect;
    public float duration;

    [Header("Effect Values")]
    public int damagePerSecond;
    public float slowPercentage;
    public float stunDuration;

    public void ApplyEffect(GameObject target)
    {
        // Check if the target has an enemyMeleeAttack component and is not dead
        if (target.TryGetComponent(out enemyMeleeAttack enemy) && !enemy.isDead)
        {
            // Play sound at the enemy's location
            if (soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(soundEffect, target.transform.position);
            }

            // Show hit effect
            if (particleEffect != null)
            {
                GameObject visEffect = Instantiate(particleEffect, target.transform.position, Quaternion.identity);
                visEffect.transform.parent = target.transform;
                Destroy(visEffect, duration);
            }

            // Adjust the status effect duration based on the enemy tier
            float adjDur = TierAdjustment(duration, (EnemyTier)enemy.enemyTier);

            // Apply the appropriate status effect
            switch (effectType)
            {
                case EffectType.Fire:
                    enemy.StartCoroutine(ApplyFireEffect(enemy, adjDur));
                    break;
                case EffectType.Ice:
                    enemy.StartCoroutine(ApplyIceEffect(enemy, adjDur));
                    break;
                case EffectType.Lightning:
                    enemy.StartCoroutine(ApplyLightingEffect(enemy, adjDur));
                    break;
            }
        }
    }

    private IEnumerator ApplyLightingEffect(enemyMeleeAttack enemy, float adjDur)
    {
        if (enemy == null || enemy.isDead)
        {
            yield break;
        }

        enemy.SetStunned(true);

        // Apply bonus damage for Lightning
        float adjustedDamage = enemy.CalculateDamage(damagePerSecond, EffectType.Lightning);
        enemy.takeDamage((int)adjustedDamage, null, EffectType.Lightning);

        yield return new WaitForSeconds(adjDur);

        enemy.SetStunned(false);
    }

    private IEnumerator ApplyIceEffect(enemyMeleeAttack enemy, float adjDur)
    {
        if (enemy == null || enemy.isDead)
        {
            yield break;
        }

        float originalSpeed = enemy.GetAgentSpeed();
        float slowSpeed = originalSpeed * (1 - slowPercentage);

        // Apply the slow effect
        enemy.ModifySpeed(slowSpeed);

        // Apply bonus damage for Ice
        float adjustedDamage = enemy.CalculateDamage(damagePerSecond, EffectType.Ice);
        enemy.takeDamage((int)adjustedDamage, null, EffectType.Ice);

        yield return new WaitForSeconds(adjDur);

        enemy.ResetSpeed();
    }

    private IEnumerator ApplyFireEffect(enemyMeleeAttack enemy, float adjDur)
    {
        if (enemy == null || enemy.isDead)
        {
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < adjDur)
        {
            // Apply bonus damage for Fire
            float adjustedDamage = enemy.CalculateDamage(damagePerSecond, EffectType.Fire);
            enemy.takeDamage((int)adjustedDamage, null, EffectType.Fire);

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
    }

    private float TierAdjustment(float duration, EnemyTier enemyTier)
    {
        switch (enemyTier)
        {
            case EnemyTier.Tier1:
                return duration;
            case EnemyTier.Tier2:
                return duration * 0.75f;
            case EnemyTier.Tier3:
                return duration * 0.5f;
            case EnemyTier.Tier4:
                return duration * 0.25f;
            default:
                return duration;
        }
    }
}
