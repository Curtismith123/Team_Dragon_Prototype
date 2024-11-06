using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{

    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;

    float rotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;

        if(invertY) 
            rotX += mouseY;
        else
            rotX -= mouseY;
        // clamp camera x rot
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);
        // rotatate cam on x
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        // rotatate cam on y
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
