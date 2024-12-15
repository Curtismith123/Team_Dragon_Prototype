using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform startpoint;
    public Transform endpoint;
    public float speed;
    public float pauseduration;
    private bool atEnd = false;

    private void Start()
    {
        // on start this has the position of the start point 
        transform.position = startpoint.position;
        StartCoroutine(MovePlatform());

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.SetParent(this.transform);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
    private IEnumerator MovePlatform()
    {
        while (true)
        {
            // Move to the end point 

            Transform constant = atEnd ? startpoint : endpoint;

            while (Vector3.Distance(transform.position, constant.position) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, constant.position, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = constant.position;
            yield return new WaitForSeconds(pauseduration);

            atEnd = !atEnd;

        }
    }
}
