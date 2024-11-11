using System.Collections;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] GameObject ponitA;
    [SerializeField] GameObject pointB;
    [SerializeField] float speed = 10f;
    [SerializeField] float delay = 1f;
    [SerializeField] GameObject Object;

    private Vector3 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Object.transform.position = ponitA.transform.position;
        targetPosition = pointB.transform.position;
        StartCoroutine(Move());
    }

    IEnumerator Move() 
    {
        while (true)
        {
            while ((targetPosition - Object.transform.position).sqrMagnitude > 0.01f)
            {
                Object.transform.position = Vector3.MoveTowards(Object.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            targetPosition = targetPosition == ponitA.transform.position ? pointB.transform.position : ponitA.transform.position;

            yield return new WaitForSeconds(delay);
        }
    }

}
