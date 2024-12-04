using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Scriptable Objects/StatusEffectSO")]
public class StatusEffectSO : ScriptableObject
{
    public enum EffectType { Fire, Ice, Lightning }

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
        //play sound at location 
        if (soundEffect != null)
        {
            // play sound at enemy location 
            AudioSource.PlayClipAtPoint(soundEffect, target.transform.position);

        }
        // show hit assigned hit effect 
        if (particleEffect != null)
        {
            GameObject visEffect = Instantiate(particleEffect, target.transform.position, Quaternion.identity);
            Destroy(visEffect, duration);
        }
        if (target.TryGetComponent(out enemyMeleeAttack enemy))
        {
            // adjust the status effect duration based on enemy tier 
            float adjDur = TierAdjustment(duration, (EnemyTier)enemy.enemyTier);

            // apply effect based on attack type 
            if (enemy != null && !enemy.isDead)
            {
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
    }

    private IEnumerator ApplyLightingEffect(enemyMeleeAttack enemy, float adjDur)
    {

        enemy.SetStunned(true);
        yield return new WaitForSeconds(adjDur);
        enemy.SetStunned(false);

    }


    private IEnumerator ApplyIceEffect(enemyMeleeAttack enemy, float adjDur)
    {

        float originalSpeed = enemy.GetAgentSpeed();
        float slowSpeed = originalSpeed * (1 - slowPercentage);


        enemy.ModifySpeed(slowSpeed);
        yield return new WaitForSeconds(adjDur);
        enemy.ResetSpeed();

    }


    private IEnumerator ApplyFireEffect(enemyMeleeAttack enemy, float adjDur)
    {
        if (enemy == null || enemy.isDead)
        {
            yield break;
        }
        float elapsedtime = 0f;
        while (elapsedtime < adjDur)
        {
            enemy.takeDamage(damagePerSecond, null);
            yield return new WaitForSeconds(1f);
            elapsedtime += 1f;

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
