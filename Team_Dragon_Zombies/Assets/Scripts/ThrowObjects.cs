// ThrowObjects.cs
using UnityEngine;
using UnityEngine.UI;

public class ThrowObjects : MonoBehaviour
{
    [Header("Reference")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    [Header("Settings")]
    [Range(3, 3)] public int totalThrows; // Set the initial number of throws
    public float throwCooldown;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    private int remainingThrows; // Track remaining throws
    bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
        remainingThrows = totalThrows; // Initialize remaining throws
        gameManager.instance.UpdateThrowers(remainingThrows); // Set the initial fill bar state
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

        // Decrement remaining throws
        remainingThrows--;
        gameManager.instance.UpdateThrowers(remainingThrows); // Update the fill bar immediately

        // Implement throwCooldown
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    public int GetTotalThrows()
    {
        return totalThrows;
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }
}
