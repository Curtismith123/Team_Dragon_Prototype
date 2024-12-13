using UnityEngine;

public class AimWithCamera : MonoBehaviour
{
    public Transform cameraTransform; // Assign the camera in the Inspector
    public bool isAiming = false;     // Controlled by input
    public float pitchCorrection = 90f; // Tunable correction for the pitch
    public float minClamp = -60f;     // Minimum pitch angle (look down limit)
    public float maxClamp = 60f;      // Maximum pitch angle (look up limit)

    void Update()
    {
        if (Input.GetButtonDown("Aim"))
        {
            isAiming = true;
        }
        else if (Input.GetButtonUp("Aim"))
        {
            isAiming = false;
        }
    }

    void LateUpdate()
    {
        if (!isAiming || cameraTransform == null)
            return;

        // Get the camera's pitch and apply the correction
        Vector3 cameraEulerAngles = cameraTransform.eulerAngles;
        float correctedPitch = cameraEulerAngles.x - pitchCorrection;

        // Clamp the corrected pitch to stay within the defined range
        correctedPitch = ClampAngle(correctedPitch, minClamp, maxClamp);

        // Apply the clamped rotation to this bone
        transform.localRotation = Quaternion.Euler(correctedPitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    // Helper function to clamp angles within a specified range
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f; // Convert angles > 180 to negative values
        return Mathf.Clamp(angle, min, max);
    }
}


