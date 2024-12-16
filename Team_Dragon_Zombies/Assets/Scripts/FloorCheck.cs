using UnityEngine;

public class FloorCheck : MonoBehaviour
{
    private EyeBatBoss boss;

    void Start()
    {
        boss = GetComponentInParent<EyeBatBoss>();
        if (boss == null)
        {
            Debug.LogError("Floor check not on eye boss object.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (boss == null) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            Debug.Log("Floor hit, destroying boss.");
            boss.DestroyBossNow();
        }
    }
}
