using System.Collections;
using UnityEngine;

public class UITextCollider : MonoBehaviour
{
    [SerializeField] GameObject uiObject;
    public int secToWait;
    void Start()
    {
        uiObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            uiObject.SetActive(true);
            StartCoroutine("WaitForSec");
        }
    }

    IEnumerator WaitForSec()
    {
        yield return new WaitForSeconds(secToWait);
        Destroy(uiObject);
        Destroy(gameObject);

    }

}
