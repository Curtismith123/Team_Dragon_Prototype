using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public float speed = 1.5f; // Speed of the pendulum's swing
    public float forwardLimit = 75f; // Max forward angle
    public float backwardLimit = -75; // Max backward angle 
    public bool randomStart = false; // Whether to start the pendulum at a random point in its swing
    public float pushForce = 10f; // Force applied to the player on collision

    private float random = 0;
    private Rigidbody rb;

    private Vector3 InitialPostiion;
    private Vector3 InitialRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitialPostiion = transform.position;
        InitialRotation = transform.localEulerAngles;
        // Starting the random offset if randomStart is enabled
        if (randomStart)
        {
            random = Random.Range(0f, 1f);
        }
        // Get or Add Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the angle of the pendulum using Mathf.Lerp and Math.Sin
        float angle = Mathf.Lerp(backwardLimit, forwardLimit, (Mathf.Sin(Time.time + random * speed) + 1f) / 2f);
        // combine the initial rotation with the penulums z-axis rotation
        // Keep the initial rotation and apply the swing to the Z-axis
        transform.localEulerAngles = new Vector3(
            InitialRotation.x,
            InitialRotation.y,
            InitialRotation.z + angle);


        transform.localRotation = Quaternion.Euler(0, 0, angle); // Apply the calculated angle to the pendulum's rotation
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handling collsion with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Rigidbody component of the player
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Calculate the push direction and apply force to the player
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
        }
    }
}
