using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public static cameraController camController;

    private PlayerController player;

    [Header("Camera Settings")]
    [SerializeField] int sensitivity = 100;
    [SerializeField] int lockVertMin = -80, lockVertMax = 80; // Vertical rotation limits
    [SerializeField] bool invertY = false;

    [Header("Collision Settings")]
    [SerializeField] LayerMask collisionLayerMask; // Define what layers the camera can collide with
    [SerializeField] float cameraRadius = 0.5f; // Radius for spherecast collision
    [SerializeField] float smoothSpeed = 10f; // Speed for smoothing collision adjustments
    [SerializeField] float viewChangeDistance=1.5f; // Distance at which to change the culling mask 

    [Header("Culling Masks")]
    [SerializeField] private LayerMask defaultMask; // Default culling mask for the camera
    [SerializeField] private LayerMask collisionMask; // Culling mask to exclude player layer during collision

    [Header("Camera View Toggle")]
    [SerializeField] Transform firstPersonView; // Position for first-person view
    [SerializeField] Transform overShoulderView; // Position for over-the-shoulder view
    [SerializeField] bool isFPV = false; // Current camera mode (true = first-person)

    private float rotX; // Tracks vertical rotation
    private Vector3 desiredPosition; // Desired position for collision avoidance
    private Vector3 smoothPosition; // Smoothed position for collision adjustments
    private Camera cam; // Reference to the Camera component

    public int Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }

    public bool InvertY
    {
        get { return invertY; }
        set { invertY = value; }
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the game window
        camController = this;
        cam = GetComponent<Camera>(); // Get the Camera component on this GameObject
        player = this.GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        HandleRotation();
        HandleViewToggle();
        HandleCollision();
    }

    private void HandleRotation()
    {
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        if (invertY)
        {
            rotX += mouseY;
        }
        else
        {
            rotX -= mouseY;
        }

        // Clamp vertical rotation (pitch)
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        // Apply vertical rotation to the camera
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // Apply horizontal rotation to the parent (e.g., player object)
        transform.parent.Rotate(Vector3.up * mouseX);
    }

    private void HandleViewToggle()
    {
        if (Input.GetButtonDown("camTog")) // Press key/button to toggle view
        {
            isFPV = !isFPV; // Toggle between first-person and over-the-shoulder views

            if (isFPV) { player.hat.SetActive(false); } // Disable hat if firstpersonView
            else { player.hat.SetActive(true); }        // enable hat in  over the shoulder view 
        }

        // Set desired camera position based on the current view
        desiredPosition = isFPV ? firstPersonView.position : overShoulderView.position;
    }

    private void HandleCollision()
    {
        // Direction from the player to the desired camera position
        Vector3 directionToCamera = (desiredPosition - transform.parent.position).normalized;

        // Perform a spherecast to detect collisions
        if (Physics.SphereCast(transform.parent.position, cameraRadius, directionToCamera, out RaycastHit hit, Vector3.Distance(transform.parent.position, desiredPosition), collisionLayerMask))
        {
            // If a collision is detected, adjust the camera's position
            Vector3 collisionPoint = hit.point;
            Vector3 directionToPlayer = (firstPersonView.transform.position - collisionPoint).normalized;
            desiredPosition = collisionPoint + directionToPlayer * cameraRadius;
            float distance = Vector3.Distance(firstPersonView.transform.position, this.transform.position);

            if (distance < viewChangeDistance)
            {
                // Change the culling mask to avoid rendering the player layer
                cam.cullingMask = collisionMask.value;
            }
        }
        else
        {
            // Restore the default culling mask when no collision is detected
            cam.cullingMask = defaultMask.value;
        }

        // Smoothly move the camera to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }
}
