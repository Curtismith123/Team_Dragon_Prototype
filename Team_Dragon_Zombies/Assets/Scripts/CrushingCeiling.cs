using System.Collections;
using System.Collections.Generic;
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

    [Header("Layer Settings")]
    [Tooltip("Layer name for CrushingCeiling.")]
    public string crushingCeilingLayerName = "CrushingCeiling";
    [Tooltip("Layer name for CrushingCeilingAfterCrush.")]
    public string crushingCeilingAfterCrushLayerName = "CrushingCeilingAfterCrush";

    private bool isCrushing = false;
    private bool hasCrushedPlayer = false;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Rigidbody rb;

    private Collider ceilingCollider;
    private Collider damageTriggerCollider;

    private int crushingCeilingLayer;
    private int crushingCeilingAfterCrushLayer;

    private List<Collider> floorColliders = new List<Collider>();
    private Collider playerCollider;

    void Start()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.down * crushDistance;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log("CrushingCeiling: Rigidbody configured as Kinematic with gravity disabled.");
        }
        else
        {
            Debug.LogWarning("CrushingCeiling requires a Rigidbody component.");
        }

        ceilingCollider = GetComponent<Collider>();
        if (ceilingCollider != null)
        {
            if (ceilingCollider.isTrigger)
            {
                Debug.LogError("CrushingCeiling: Main collider should not be a trigger.");
            }
            else
            {
                Debug.Log("CrushingCeiling: Main collider found.");
            }
        }
        else
        {
            Debug.LogError("CrushingCeiling: No Collider component found.");
        }

        if (transform.Find("DamageTrigger") != null)
        {
            damageTriggerCollider = transform.Find("DamageTrigger").GetComponent<Collider>();
            if (damageTriggerCollider != null && damageTriggerCollider.isTrigger)
            {
                Debug.Log("CrushingCeiling: DamageTrigger collider found and set as trigger.");
            }
            else
            {
                Debug.LogError("CrushingCeiling: DamageTrigger collider not found or not set as trigger.");
            }
        }
        else
        {
            Debug.LogError("CrushingCeiling: DamageTrigger child GameObject not found.");
        }

        if (tripTrigger != null)
        {
            CrushingCeilingTrigger trigger = tripTrigger.GetComponent<CrushingCeilingTrigger>();
            if (trigger != null)
            {
                trigger.OnTriggered += ActivateCrushing;
                Debug.Log("CrushingCeiling: Registered ActivateCrushing with trip trigger.");
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

        crushingCeilingLayer = LayerMask.NameToLayer(crushingCeilingLayerName);
        crushingCeilingAfterCrushLayer = LayerMask.NameToLayer(crushingCeilingAfterCrushLayerName);

        if (crushingCeilingLayer == -1)
        {
            Debug.LogError($"CrushingCeiling: Layer '{crushingCeilingLayerName}' not found. Please add it in Tags and Layers.");
        }

        if (crushingCeilingAfterCrushLayer == -1)
        {
            Debug.LogError($"CrushingCeiling: Layer '{crushingCeilingAfterCrushLayerName}' not found. Please add it in Tags and Layers.");
        }

        if (crushingCeilingLayer != -1)
        {
            gameObject.layer = crushingCeilingLayer;
            Debug.Log($"CrushingCeiling: Set layer to '{crushingCeilingLayerName}'.");
        }

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (((1 << obj.layer) & floorLayer) != 0)
            {
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    floorColliders.Add(col);
                    Debug.Log($"CrushingCeiling: Cached floor collider '{obj.name}'.");
                }
            }
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null)
            {
                Debug.Log($"CrushingCeiling: Cached player collider '{player.name}'.");
            }
            else
            {
                Debug.LogError("CrushingCeiling: Player GameObject lacks a Collider component.");
            }
        }
        else
        {
            Debug.LogError("CrushingCeiling: No GameObject tagged 'Player' found.");
        }
    }

    void ActivateCrushing()
    {
        isCrushing = true;
        Debug.Log("CrushingCeiling: Activated crushing movement.");
    }

    void FixedUpdate()
    {
        if (isCrushing)
        {
            if (hasCrushedPlayer)
            {

                Vector3 newPosition = rb.position + Vector3.down * crushSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
                Debug.Log($"CrushingCeiling: Continuing to move downward to {newPosition}");
            }
            else
            {

                Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, crushSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPosition);
                Debug.Log($"CrushingCeiling: Moving to {targetPosition}, Current Position: {rb.position}");

                if (Vector3.Distance(rb.position, targetPosition) < 0.001f)
                {
                    isCrushing = false;
                    Debug.Log("CrushingCeiling: Reached target position. Stopping movement.");
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"CrushingCeiling: Collided with '{collision.gameObject.name}' on layer {collision.gameObject.layer} ({LayerMask.LayerToName(collision.gameObject.layer)})");

        if (((1 << collision.gameObject.layer) & floorLayer) != 0)
        {
            if (hasCrushedPlayer)
            {

                Physics.IgnoreCollision(ceilingCollider, collision.collider);
                Debug.Log($"CrushingCeiling: Ignored collision with floor '{collision.gameObject.name}'. Continuing movement.");
            }
            else
            {

                StopCrushing();
                Debug.Log($"CrushingCeiling: Collided with floor object '{collision.gameObject.name}'. Stopping crushing.");
            }
            return;
        }

        IDamage damageable = collision.gameObject.GetComponent<IDamage>();
        if (isCrushing && damageable != null)
        {

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.IsGrounded)
            {

                damageable.takeDamage(crushDamage, gameObject);
                Debug.Log($"CrushingCeiling: Applied {crushDamage} damage to grounded player '{collision.gameObject.name}'.");
            }
            else
            {
                Debug.Log($"CrushingCeiling: Collision with '{collision.gameObject.name}' is not grounded or lacks PlayerController.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.IsGrounded && !hasCrushedPlayer)
        {

            player.takeDamage(crushDamage, gameObject);
            Debug.Log($"CrushingCeiling: Applied {crushDamage} damage to grounded player '{other.gameObject.name}' via trigger.");

            hasCrushedPlayer = true;

            DisableAllColliders();

            ChangeLayerAfterCrush();

            IgnoreFloorAndPlayerCollisions();
        }
    }

    void DisableAllColliders()
    {

        if (ceilingCollider != null && ceilingCollider.enabled)
        {
            ceilingCollider.enabled = false;
            Debug.Log("CrushingCeiling: Disabled main collider to allow passing through the floor.");
        }

        foreach (Collider childCollider in GetComponentsInChildren<Collider>())
        {
            if (childCollider != damageTriggerCollider && childCollider.enabled)
            {
                childCollider.enabled = false;
                Debug.Log($"CrushingCeiling: Disabled child collider '{childCollider.gameObject.name}'.");
            }
        }

        if (damageTriggerCollider != null && damageTriggerCollider.enabled)
        {
            damageTriggerCollider.enabled = false;
            Debug.Log("CrushingCeiling: Disabled DamageTrigger collider after crushing the player.");
        }
    }

    void ChangeLayerAfterCrush()
    {
        if (crushingCeilingAfterCrushLayer != -1)
        {
            gameObject.layer = crushingCeilingAfterCrushLayer;
            Debug.Log($"CrushingCeiling: Changed layer to '{LayerMask.LayerToName(gameObject.layer)}' to ignore floor collisions.");
        }

        if (damageTriggerCollider != null)
        {
            damageTriggerCollider.gameObject.layer = crushingCeilingAfterCrushLayer;
            Debug.Log($"CrushingCeiling: Changed DamageTrigger's layer to '{LayerMask.LayerToName(crushingCeilingAfterCrushLayer)}'.");
        }
    }

    void IgnoreFloorAndPlayerCollisions()
    {
        foreach (Collider floorCollider in floorColliders)
        {
            if (floorCollider != null)
            {
                Physics.IgnoreCollision(ceilingCollider, floorCollider);
                Debug.Log($"CrushingCeiling: Ignored collision with floor '{floorCollider.gameObject.name}'.");
            }
        }

        if (playerCollider != null)
        {
            Physics.IgnoreCollision(ceilingCollider, playerCollider);
            Debug.Log($"CrushingCeiling: Ignored collision with player '{playerCollider.gameObject.name}'.");
        }
    }

    void StopCrushing()
    {
        isCrushing = false;
        Debug.Log("CrushingCeiling: Stopped crushing movement upon floor collision.");
    }
}
