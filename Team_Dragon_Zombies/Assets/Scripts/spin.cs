using UnityEngine;

public class spin : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        transform.Rotate(0f, 25f * Time.deltaTime, 0f, Space.Self); // To Spin the objects
    }
}