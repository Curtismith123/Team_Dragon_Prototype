using UnityEngine;

public class DestroyActivate : MonoBehaviour
{
    public GameObject objectToDestroy;
    public GameObject objectToActivate;
    public float delayTime = 2f;

    public void OnObjectDestroyed()
    {
        Invoke("ActivateObject", delayTime);
    }

    private void ActivateObject()
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log("Object activated!");
        }
    }
}