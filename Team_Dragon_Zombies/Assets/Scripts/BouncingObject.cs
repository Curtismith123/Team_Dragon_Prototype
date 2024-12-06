using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BouncingObject : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Constant speed of the object.")]
    public float speed = 5f;

    [Tooltip("Empty GameObject for initial direction.")]
    public Transform initialDirection;

    private Rigidbody rb;

    private bool canReflect = true;
    private float reflectionCooldown = 0.1f;

    void Start()
    {
        if (initialDirection == null)
        {
            Debug.LogError("Initial Direction is not assigned.");
            enabled = false;
            return;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody missing.");
            enabled = false;
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 direction = (initialDirection.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!canReflect)
            return;

        canReflect = false;

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        Vector3 reflectedVelocity = Vector3.Reflect(rb.linearVelocity, normal).normalized * speed;

        rb.linearVelocity = reflectedVelocity;

        Invoke(nameof(ResetReflection), reflectionCooldown);
    }

    void ResetReflection()
    {
        canReflect = true;
    }

    void OnDrawGizmos()
    {
        if (initialDirection != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, initialDirection.position);
        }

        if (rb != null && rb.linearVelocity != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2);
        }
    }
}
