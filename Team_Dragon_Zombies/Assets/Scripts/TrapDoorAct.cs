
using System.Collections;
using UnityEngine;

public class TrapDoorAct : MonoBehaviour
{
    public GameObject TrapDoor;
    public Camera Camera;
    public float range = 3f;
    public float open = 100f; // Angle to open the trapdoor to the floor
    public bool isOpen = false;
   

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            Aim();
        }

        if (Input.GetKeyDown("f") & isOpen == true) 
        {
            AimTwo();
        }
    }

    void Aim()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            ActTrap trap = hit.transform.GetComponent<ActTrap>();
            if (trap != null)
            {
                TrapDoor.transform.eulerAngles = new Vector3(TrapDoor.transform.eulerAngles.x - 90, TrapDoor.transform.eulerAngles.y, TrapDoor.transform.eulerAngles.z);
            }
        }
    }

    void AimTwo()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            ActTrap trap = hit.transform.GetComponent<ActTrap>();
            if (trap != null)
            {
                TrapDoor.transform.eulerAngles = new Vector3(TrapDoor.transform.eulerAngles.x + 90, TrapDoor.transform.eulerAngles.y, TrapDoor.transform.eulerAngles.z);
            }
        }
    }
}
