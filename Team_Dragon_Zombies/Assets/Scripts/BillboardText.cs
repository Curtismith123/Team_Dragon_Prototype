using UnityEngine;
using TMPro;

public class BillboardText : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

            transform.Rotate(0, 180, 0);
        }
    }
}