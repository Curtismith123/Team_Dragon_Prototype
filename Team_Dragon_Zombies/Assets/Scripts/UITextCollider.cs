using System.Collections;
using UnityEngine;

public class UITextCollider : MonoBehaviour
{
    [SerializeField] GameObject uiObject;
    public int secToWait;

    private bool hasTriggered = false;
    void Start()
    {
        uiObject.SetActive(false);
    }

    private void Update()
    {
        if (hasTriggered)
        {
            uiObject.SetActive(!gameManager.instance.isPaused);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !gameManager.instance.isPaused)
        {
            hasTriggered = true;
            uiObject.SetActive(true);
            StartCoroutine("WaitForSec");
        }
    }

    IEnumerator WaitForSec()
    {
        yield return new WaitForSeconds(secToWait);
        if (!gameManager.instance.isPaused)
        {
            Destroy(uiObject);
            Destroy(gameObject);
        }

    }

}
