using UnityEngine;

public class spin : MonoBehaviour
{
    [SerializeField] private Vector3 spinAxis = Vector3.up; //spin x, y, or z axis
    [SerializeField] private float spinSpeed = 25f; //speed
    [SerializeField] private bool clockwise = true;

    void Update()
    {
        float direction = clockwise ? 1f : -1f;
        transform.Rotate(spinAxis * spinSpeed * direction * Time.deltaTime, Space.Self);
    }
}