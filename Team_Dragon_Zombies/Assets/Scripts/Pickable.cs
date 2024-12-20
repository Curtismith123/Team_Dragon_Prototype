using UnityEngine;

public class Pickable : MonoBehaviour
{
    public ThrowObjects throwObjectsScript;
    public bool canPickUp = true;
    public float pickupRange = 2f;

    private bool isPickedUp = false;


    private void Start()
    {
        if (throwObjectsScript == null)
        {
            throwObjectsScript = FindAnyObjectByType<ThrowObjects>();
            if (throwObjectsScript == null)
            {
                Debug.LogError("No throwobject script located in scene");
            }
        }
    }

    private void Update()
    {
        throwObjectsScript = gameManager.instance.playerScript.GetComponent<ThrowObjects>();

        if (Input.GetKeyDown(KeyCode.E) && canPickUp && !isPickedUp) //backup pickup system if not auto picking up.
        {
            TryPickup();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (canPickUp && !isPickedUp && other.CompareTag("Player"))
        {
            if (Vector3.Distance(other.transform.position, transform.position) <= pickupRange)
            {
                Pickup(other.transform);
            }
        }
    }

    private void TryPickup()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                Pickup(collider.transform);
                return;
            }
        }
    }

    private void Pickup(Transform player)
    {
        throwObjectsScript.AddThrow();
        isPickedUp = true;
        Destroy(gameObject);
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