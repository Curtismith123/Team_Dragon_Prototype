using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    public float life = 5f;

   void Awake()
    {
        Destroy(gameObject,life);
    }

}
