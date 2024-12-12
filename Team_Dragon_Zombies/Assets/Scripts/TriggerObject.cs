using UnityEngine;

public class TriggerObject : MonoBehaviour
{
    [SerializeField] MoveObject platform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.canMove = true;
            Debug.Log("Player entered trigger, platform can move now. ");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.canMove = false;
            Debug.Log("Player exited trigger, platform stops moving. ");
        }
    }
}
