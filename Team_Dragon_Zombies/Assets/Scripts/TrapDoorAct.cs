
using System.Collections;
using UnityEngine;

public class TrapDoorAct : MonoBehaviour
{
    public GameObject[] TrapDoors;
    public Camera Camera;
    public float range = 3f;
    public float openAngle = 90f; // Angle to open the trapdoor to the floor
    public bool[] isOpen;
    public float openingSpeed = 2f;

    private Vector3[] closedRotaion;
    private Vector3[] openRotaion;
   

    void Start()
    {
        isOpen = new bool[TrapDoors.Length];
        closedRotaion = new Vector3[TrapDoors.Length];
        openRotaion = new Vector3[TrapDoors.Length];

        for (int i = 0; i < TrapDoors.Length; i++)
        {
            closedRotaion[i] = TrapDoors[i].transform.eulerAngles;
            openRotaion[i] = new Vector3(closedRotaion[i].x - openAngle, closedRotaion[i].y, closedRotaion[i].z);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, range))
            {
                Debug.Log(hit.transform.name);
                ActTrap trap = hit.transform.GetComponent<ActTrap>();
                if (trap != null)
                {
                    for (int i = 0; i < TrapDoors.Length; ++i)
                    {
                        if (TrapDoors[i] == trap.gameObject)
                        {
                            if (!isOpen[i])
                            {
                                StartCoroutine(RotateTrapDoor(i, TrapDoors[i].transform.eulerAngles, openRotaion[i]));
                                isOpen[i] = true;
                            }  
                            else
                            {
                                StartCoroutine(RotateTrapDoor(i, TrapDoors[i].transform.eulerAngles, closedRotaion[i]));
                                isOpen[i] = false;
                            }
                        }
                    }
                }
            }
        }
       
    }

    

    IEnumerator RotateTrapDoor (int index, Vector3 fromAngle, Vector3 toAngle)
    {
        float elapsedTime = 0f;
        Vector3 targetAngle = toAngle;

        if (!isOpen[index])
        {
            targetAngle = new Vector3(fromAngle.x + openAngle, fromAngle.y, fromAngle.z);
        }
        
        while (elapsedTime < openingSpeed)
        {
            TrapDoors[index].transform.eulerAngles = Vector3.Lerp(fromAngle, toAngle, elapsedTime / openingSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        TrapDoors[index].transform.eulerAngles = toAngle;
    }
 }
