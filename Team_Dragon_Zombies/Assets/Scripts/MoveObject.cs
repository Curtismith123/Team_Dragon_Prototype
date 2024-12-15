using System.Collections;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public bool canMove;

    [SerializeField] float speed;
    [SerializeField] int startPoint;
    [SerializeField] Transform[] points;

    int i;
    public bool reverse = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = points[startPoint].position;
        i = startPoint;
    }

    void Update()
    {

        if (Vector3.Distance(transform.position, points[i].position) < 0.01f)
        {
            canMove = false;

            if (i == points.Length - 1)
            {
                reverse = true;
                i--;
                return;
            }
            else if (i == 0)
            {
                reverse = false;
                i++;
                return;
            }

            if (reverse)
            {
                i--;
            }
            else
            {
                i++;
            }
        }


        if (canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
        }
    }
}
