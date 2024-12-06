using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Image progressBar;
    //[SerializeField] TMP_Text progressText;
    [SerializeField] private FadeInOut fadeController;

    private float target;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadLevel();
        }
    }

    public async void LoadLevel()
    {
        //fade out
        fadeController.FadeOut();
        //wait for method to finish
        await Task.Delay((int)(fadeController.fadeDuration * 1000));

        loadingScreen.SetActive(true);

        //restart progress
        target = 0;
        progressBar.fillAmount = 0;

        var scene = SceneManager.LoadSceneAsync(sceneIndex);
        scene.allowSceneActivation = false;

        do
        {
            await Task.Delay(100);

            target = scene.progress;
        } while (scene.progress < 0.9f);

        await Task.Delay(2000);

        scene.allowSceneActivation = true;
        loadingScreen.SetActive(false);

        ////fade in after done loading
        //fadeController.FadeIn();
        //await Task.Delay((int)(fadeController.fadeDuration * 1000));

    }

    //polish the progress bar to load smoothly
    private void Update()
    {
        progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, target, 0.5f * Time.deltaTime);
    }
}