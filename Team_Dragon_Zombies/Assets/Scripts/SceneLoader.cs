using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Image progressBar;
    [SerializeField] public FadeInOut fadeController;

    private float target;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(LoadLevelCoroutine(sceneIndex));
        }
    }

    private IEnumerator LoadLevelCoroutine(int sceneIndex)
    {
        ToggleCurrentEffectIcon(false);
        if (fadeController != null)
        {
            fadeController.FadeOut();
            yield return new WaitForSeconds(fadeController.fadeDuration);
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        target = 0;
        if (progressBar != null)
            progressBar.fillAmount = 0;

        // Start loading scene
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneIndex);
        scene.allowSceneActivation = false;

        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        while (scene.progress < 0.9f)
        {
            target = scene.progress;
            yield return null; // Wait for the next frame
        }

        yield return new WaitForSeconds(2f); // Simulated load delay

        scene.allowSceneActivation = true;

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from the event to prevent duplication
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Find spawn point in the scene
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Player Spawn Pos");
        if (spawnPoint != null)
        {
            // Move the player to the spawn point position
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;
            }
           
        }
       
        ToggleCurrentEffectIcon(true);
    }

    private void Update()
    {
        if (progressBar != null)
            progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, target, 0.5f * Time.deltaTime);
    }
    private void ToggleCurrentEffectIcon(bool isVisible)
    {
        if (gameManager.instance.playerScript != null)
        {
            Image effectIcon = gameManager.instance.playerScript.currentEffectIcon;

            if (effectIcon != null)
                effectIcon.enabled = isVisible;
        }
    }


}

