using UnityEngine;

public class FloorCheck : MonoBehaviour
{
    private EyeBatBoss boss;

    void Start()
    {
        boss = GetComponentInParent<EyeBatBoss>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Floor") && boss != null)
        {
            boss.DestroyBossNow();
        }
    }
}
