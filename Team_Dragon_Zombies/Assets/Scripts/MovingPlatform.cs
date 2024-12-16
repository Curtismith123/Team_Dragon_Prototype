using System.Collections;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform startpoint;
    public Transform endpoint;
    public float speed;
    public float pauseduration;
    private bool atEnd = false;

    public bool isRandom;
    public float durMin;
    public float durMax;
    private float origSpeed;




    private void Start()
    {
        // on start this has the position of the start point 
        transform.position = startpoint.position;
        StartCoroutine(MovePlatform());
        origSpeed = speed;
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
                transform.position = Vector3.MoveTowards(transform.position, constant.position, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = constant.position;
            if (isRandom)
            {
                pauseduration = Random.Range(durMax, durMin);

                speed = Random.Range(origSpeed * 2, origSpeed / 2);
            }
            yield return new WaitForSeconds(pauseduration);
            atEnd = !atEnd;
        }

    }
}
