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
    [SerializeField] private FadeInOut fadeController;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleLoadLevel());
        }
    }

    //Code to fade in and out black before/after level loading complete
    private IEnumerator HandleLoadLevel()
    {
        if (fadeController != null)
        {
            fadeController.FadeOut();
            yield return new WaitForSeconds(fadeController.fadeDuration);
        }

        //start loading new scene
        yield return StartCoroutine(LoadLevel());


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

            progressBar.fillAmount = progress;
            progressText.text = Mathf.RoundToInt(progress * 100) + "%";

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true;

                //turn off loading screen once complete
                if (loadingScreen != null)
                {
                    loadingScreen.SetActive(false);
                }
                //After scene activation=true, fade in
                if (fadeController != null)
                {
                    fadeController.FadeIn();
                }

            }

            yield return null;

        }

    }
}