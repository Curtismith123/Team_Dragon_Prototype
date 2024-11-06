using UnityEngine;

public class spin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 25f * Time.deltaTime, 0f, Space.Self); // To Spin the objects
    }
}
