using UnityEngine;

public class TextAlwaysAbove : MonoBehaviour
{
    public Transform rollingObject;
    public Vector3 offset;

    void Update()
    {
        if (rollingObject != null)
        {
            transform.position = rollingObject.position + offset;

            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }
}
