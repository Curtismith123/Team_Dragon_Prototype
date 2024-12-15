using UnityEngine;

public class TriggerObject : MonoBehaviour
{
    [SerializeField] MoveObject platform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.canMove = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.canMove = false;
        }
    }
}
