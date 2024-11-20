using UnityEngine;

public class Pickable : MonoBehaviour
{
    public ThrowObjects throwObjectsScript;
    public bool canPickUp = true;
    public float pickupRange = 2f;

    private bool isPickedUp = false;

    private void OnTriggerStay(Collider other)
    {
        if (canPickUp && !isPickedUp && other.CompareTag("Player"))
        {
            if (Vector3.Distance(other.transform.position, transform.position) <= pickupRange)
            {
                throwObjectsScript.AddThrow();
                isPickedUp = true;
                Destroy(gameObject);
            }
        }
    }

    public void DisablePickup()
    {
        canPickUp = false;
    }

    public void EnablePickup()
    {
        canPickUp = true;
        isPickedUp = false;
    }
}