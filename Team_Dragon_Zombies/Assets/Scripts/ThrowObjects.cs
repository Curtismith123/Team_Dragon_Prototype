using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThrowObjects : MonoBehaviour
{
    [Header("Reference")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    [Header("Settings")]
    [Range(3, 3)] public int maxThrows = 3;
    public float throwCooldown;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwForce;
    public float throwUpwardForce;

    [Tooltip("Maximum downward angle (in degrees) the player can throw.")]
    public float maxDownwardAngle = -15f;

    public int remainingThrows = 0;
    private bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
        gameManager.instance.UpdateThrowers(remainingThrows);
    }

    private void Update()
    {
        if (Input.GetKeyDown(throwKey) && readyToThrow && remainingThrows > 0)
        {
            Debug.Log("Throw Key Pressed:" + throwKey);
            Throw();
        }
    }

    private void Throw()
    {
        if (remainingThrows > 0)
        {
            readyToThrow = false;

            GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

            Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();

            Vector3 forceDirection = cam.transform.forward;

            float angle = Vector3.Angle(cam.transform.forward, Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up));

            if (angle > maxDownwardAngle)
            {

                bool isLookingDown = cam.transform.forward.y < 0;

                if (isLookingDown)
                {

                    forceDirection = Quaternion.Euler(-maxDownwardAngle, cam.eulerAngles.y, 0) * Vector3.forward;
                }
            }

            RaycastHit hit;

            if (Physics.Raycast(cam.position, forceDirection, out hit, 500f))
            {
                forceDirection = (hit.point - attackPoint.position).normalized;
            }

            Vector3 forceToAdd = forceDirection * throwForce + Vector3.up * throwUpwardForce;
            projectileRB.AddForce(forceToAdd, ForceMode.Impulse);


            Pickable pickable = projectile.GetComponent<Pickable>();
            if (pickable != null)
            {
                pickable.throwObjectsScript = this;
                pickable.canPickUp = false;

                StartCoroutine(EnablePickupAfterDelay(pickable, 0.5f));
            }

            remainingThrows--;
            gameManager.instance.UpdateThrowers(remainingThrows);

            Invoke(nameof(ResetThrow), throwCooldown);
        }
    }

    private IEnumerator EnablePickupAfterDelay(Pickable pickable, float delay)
    {
        yield return new WaitForSeconds(delay);
        pickable.canPickUp = true;
    }

    public void AddThrow()
    {
        if (remainingThrows < maxThrows)
        {
            remainingThrows++;
            gameManager.instance.UpdateThrowers(remainingThrows);
        }
    }

    public void ResetThrow()
    {
        readyToThrow = true;
    }

    public int GetTotalThrows()
    {
        return maxThrows;
    }
    public void SetRemainingThrows(int count)
    {
        count = count > maxThrows ? maxThrows : count;
        remainingThrows = count;
    }
}
