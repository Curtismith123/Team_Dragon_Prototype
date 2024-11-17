using UnityEngine;

public class ItemLevitate : MonoBehaviour
{
    public float height = 0.5f;
    public float speed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * speed) * height;
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }
}