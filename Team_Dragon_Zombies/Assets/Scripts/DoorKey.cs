using UnityEngine;

public class DoorKey : MonoBehaviour
{
    [SerializeField] string requiredKeyID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameManager.instance.UseKey(requiredKeyID))
            {
                Destroy(gameObject);
                Debug.Log("Door unlocked!");
            }
            else
            {
                Debug.Log("You don't have the key to this door.");
            }
        }
    }
}