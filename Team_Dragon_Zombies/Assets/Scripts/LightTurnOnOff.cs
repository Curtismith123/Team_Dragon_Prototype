using UnityEngine;

public class LightTurnOnOff : MonoBehaviour
{

    [SerializeField] Light lightSource;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lightSource.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lightSource.enabled = false;
        }
    }

}
