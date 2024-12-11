using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    public Transform launchPoint;
    public GameObject projectile;
    public float launchVelocity = 10f;
    public KeyCode fireKey = KeyCode.F; // Default to Space Key, but can be changed in the Inspector

    private void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            if (launchPoint == null || projectile == null)
            {
                Debug.LogError("LaunchPoint or projectile is not assigned.");
                return;
            }
            var _projectile = Instantiate(projectile, launchPoint.position, launchPoint.rotation);
            Rigidbody rb = _projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = launchPoint.up * launchVelocity;
            } 
            else
            {
                Debug.LogError("Projectile does not have a Rigidbody component. ");
                Destroy(_projectile);
            }
        }
    }
}
