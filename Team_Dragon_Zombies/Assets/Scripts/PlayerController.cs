using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
   
    [SerializeField] CharacterController controller;

    [SerializeField] int speed;
    [SerializeField] int sprintMod;

    Vector3 moveDir;

    bool isSprinting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }
    void movement()
    {
        //moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        moveDir = (transform.forward * Input.GetAxis("Vertical")) +
                  (transform.right * Input.GetAxis("Horizontal"));

        controller.Move(moveDir * speed * Time.deltaTime);
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }
}


