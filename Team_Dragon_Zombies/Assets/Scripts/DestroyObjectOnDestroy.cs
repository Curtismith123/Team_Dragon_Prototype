using UnityEngine;

public class DestroyObjectOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject[] objectsToDestroy;

    private void OnDestroy()
    {
        if (objectsToDestroy != null && objectsToDestroy.Length > 0)
        {
            foreach (GameObject obj in objectsToDestroy)
            {
                if (obj != null)
                {
                    Debug.Log($"Destroying object: {obj.name} because {gameObject.name} was destroyed.");
                    Destroy(obj);
                }
            }
        }
        else
        {
            Debug.LogWarning("No objects assigned to destroy.");
        }
    }

    private void OnEnable()
    {
        if (targetObject == null)
        {
            Debug.LogError("No target object assigned. This script will not function.");
            return;
        }

        targetObject.GetComponent<DestroyHelper>()?.RegisterOnDestroyed(() =>
        {
            if (objectsToDestroy != null && objectsToDestroy.Length > 0)
            {
                foreach (GameObject obj in objectsToDestroy)
                {
                    if (obj != null)
                    {
                        Debug.Log($"{targetObject.name} was destroyed. Destroying {obj.name}.");
                        Destroy(obj);
                    }
                }
            }
            else
            {
                Debug.LogWarning("No objects assigned to destroy.");
            }
        });
    }
}