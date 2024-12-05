using UnityEngine;

public class SuperBounce : MonoBehaviour
{
    public float extraBounceMultiplier = 0.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                Vector3 additionalForce = contact.normal * rb.linearVelocity.magnitude * extraBounceMultiplier;

                float maxAdditionalForce = 10f;
                if (additionalForce.magnitude > maxAdditionalForce)
                {
                    additionalForce = additionalForce.normalized * maxAdditionalForce;
                }

                rb.AddForce(additionalForce, ForceMode.Impulse);
                break;
            }
        }
    }
}
