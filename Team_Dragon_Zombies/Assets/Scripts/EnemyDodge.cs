using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDodge : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeChance = 0.7f; //70% by default '0.7'
    private bool isDodging = false;

    public void AttemptDodge()
    {
        if (!isDodging && Random.value < dodgeChance)
        {
            StartCoroutine(Dodge());
        }
    }

    private IEnumerator Dodge()
    {
        isDodging = true;

        //randomly dodge left/right
        Vector3 dodgeDirection = Random.value > 0.5f ? Vector3.right : Vector3.left;
        Vector3 dodgeTarget = transform.position + dodgeDirection * dodgeDistance;

        agent.SetDestination(dodgeTarget);

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        isDodging = false;
    }
}
