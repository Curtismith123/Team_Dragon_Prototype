using System.Collections;
using UnityEngine;

public class CrushingCeiling : MonoBehaviour
{
    [Header("Trip Settings")]
    [Tooltip("Assign the trip trigger GameObject here.")]
    public GameObject tripTrigger;

    [Header("Crushing Settings")]
    [Tooltip("Speed at which the ceiling will crush downward.")]
    public float crushSpeed = 5f;

    [Tooltip("Distance the ceiling will move downward when crushing.")]
    public float crushDistance = 10f;

    [Tooltip("Amount of damage to apply to the player when crushed.")]
    public int crushDamage = 999;

    [Header("Floor Detection Settings")]
    [Tooltip("LayerMask to identify the floor.")]
    public LayerMask floorLayer;

    private bool isCrushing = false;
    private Vector3 initialPosition;
    private Vector3 targetPosition;

    void Start()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.down * crushDistance;

        // Ensure Rigidbody is set to Kinematic and gravity is disabled
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            Debug.LogWarning("CrushingCeiling requires a Rigidbody component.");
        }

        // Register with trip trigger
        if (tripTrigger != null)
        {
            CrushingCeilingTrigger trigger = tripTrigger.GetComponent<CrushingCeilingTrigger>();
            if (trigger != null)
            {
                trigger.OnTriggered += ActivateCrushing;
            }
            else
            {
                Debug.LogError("CrushingCeilingTrigger script not found on tripTrigger object.");
            }
        }
        else
        {
            Debug.LogError("TripTrigger not assigned in CrushingCeiling.");
        }
    }

    /// <summary>
    /// Activates the crushing movement.
    /// </summary>
    void ActivateCrushing()
    {
        isCrushing = true;
    }

    void Update()
    {
        if (isCrushing)
        {
            float step = crushSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // Stop crushing when target position is reached
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                isCrushing = false;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the floor to stop crushing
        if (((1 << collision.gameObject.layer) & floorLayer) != 0)
        {
            StopCrushing();
            return;
        }

        // Check if the collided object implements IDamage
        IDamage damageable = collision.gameObject.GetComponent<IDamage>();
        if (isCrushing && damageable != null)
        {
            // Attempt to retrieve the PlayerController to check if grounded
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.controller.isGrounded)
            {
                // Apply damage
                damageable.takeDamage(crushDamage, gameObject);
            }
            else
            {
                // Optionally handle other objects that implement IDamage
                // and have their own grounded logic
                // For now, only players with PlayerController are handled
            }
        }
    }

    /// <summary>
    /// Stops the crushing movement.
    /// </summary>
    void StopCrushing()
    {
        isCrushing = false;
    }
}
