using UnityEngine;

public class TrapDoor : MonoBehaviour
{

    public float speed;
    public float angle;
    public Vector3 direction;
    
   

    private void Start()
    {
        angle = transform.eulerAngles.x;
        
    }

    private void Update()
    {
       if (Mathf.Round(transform.eulerAngles.x) != angle)
       {
            transform.Rotate(direction * speed * Time.deltaTime);
       }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is near the door");
            OpenTrapDoor();
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left the door area");
            CloseTrapDoor();
        }
    }

    public void OpenTrapDoor()
    {
        angle = 0;
        direction = -Vector3.right;
        Debug.Log("OpenTrapDoor called, targetAngle set to " + angle);
    }

    public void CloseTrapDoor() 
    {
        angle = 90;
        direction = Vector3.right;
        Debug.Log("CloseTrapDoor called, targetAngle set to " + angle);
    }

}
