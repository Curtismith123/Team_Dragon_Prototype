using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    public float life = 5f;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

}
