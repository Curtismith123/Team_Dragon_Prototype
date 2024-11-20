using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThrowObjects : MonoBehaviour
{
    [Header("Reference")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    [Header("Settings")]
    [Range(3, 3)] public int maxThrows = 3; // Max throws player can hold
    public float throwCooldown;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    private int remainingThrows = 0; // Start with zero throws
    private bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
        gameManager.instance.UpdateThrowers(remainingThrows); // Update the UI
    }

    private void Update()
    {
        if (Input.GetKeyDown(throwKey) && readyToThrow && remainingThrows > 0)
        {
            Throw();
        }
    }

    private void Throw()
    {
        if (remainingThrows > 0) // Ensure throws count is valid
        {
            readyToThrow = false;

            // Instantiate object to throw
            GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

            // Get Rigidbody component
            Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();

            // Calculate direction
            Vector3 forceDirection = cam.transform.forward;
            RaycastHit hit;

            if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
            {
                forceDirection = (hit.point - attackPoint.position).normalized;
            }

            // Add force
            Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
            projectileRB.AddForce(forceToAdd, ForceMode.Impulse);

            // Access the Pickable component already attached to the prefab
            Pickable pickable = projectile.GetComponent<Pickable>();
            if (pickable != null)
            {
                pickable.throwObjectsScript = this;
                pickable.canPickUp = false;  // Disable pickup immediately after throw

                // Start a timer to enable pickup after a delay
                StartCoroutine(EnablePickupAfterDelay(pickable, 0.5f)); // 0.5-second delay
            }

            // Decrement remaining throws immediately after throwing
            remainingThrows--;
            gameManager.instance.UpdateThrowers(remainingThrows); // Update UI

            // Implement throw cooldown
            Invoke(nameof(ResetThrow), throwCooldown);
        }
    }

    private IEnumerator EnablePickupAfterDelay(Pickable pickable, float delay)
    {
        yield return new WaitForSeconds(delay);
        pickable.canPickUp = true;
    }

    public void AddThrow()
    {
        if (remainingThrows < maxThrows)
        {
            remainingThrows++;
            gameManager.instance.UpdateThrowers(remainingThrows); // Update UI
        }
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }

    public int GetTotalThrows()
{
    return maxThrows;
}
}
