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
            OpenTrapDoor();
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseTrapDoor();
        }
    }

    public void OpenTrapDoor()
    {
        angle = 0;
        direction = -Vector3.right;
    }

    public void CloseTrapDoor() 
    {
        angle = 90;
        direction = Vector3.right;
    }

}
