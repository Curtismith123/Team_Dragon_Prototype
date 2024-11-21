using UnityEngine;

public class DestroyTrigger : MonoBehaviour
{
    [SerializeField] private GameObject objectToDestroy;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {

            Destroy(objectToDestroy);
        }
    }
}