using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Image progressBar;
    [SerializeField] TMP_Text progressText;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadLevel());
        }
    }



    private IEnumerator LoadLevel()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        //simulating progress control
        progressBar.fillAmount = 0.5f;
        progressText.text = "50%";
        yield return new WaitForSeconds(1f);

        progressBar.fillAmount = 0.85f;
        progressText.text = "85%";
        yield return new WaitForSeconds(1f);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            //progressBar.fillAmount = progress;
            //progressText.text = Mathf.RoundToInt(progress * 100) + "%";

            if (operation.progress >= 0.9f)
            {
                //final addition to the simulation control
                progressBar.fillAmount = 1f;
                progressText.text = "100%";
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true;
            }

            yield return null;

        }
    }
}
