using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    public Transform launchPoint;
    public GameObject projectile;
    public float launchVelocity = 10f;
    public KeyCode fireKey = KeyCode.F; // Default to Space Key, but can be changed in the Inspector

   void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            var _projectile = Instantiate(projectile, launchPoint.position, launchPoint.rotation);
            _projectile.GetComponent<Rigidbody>().linearVelocity = launchPoint.up * launchVelocity;
        }
    }
}
