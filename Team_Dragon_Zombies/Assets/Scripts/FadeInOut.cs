using System.Collections;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] public float fadeDuration = 1.0f;

    public void FadeIn()
    {
        StartCoroutine(FadeCanvasGroup(1, 0)); // Fade from opaque to transparent
    }

    public void FadeOut()
    {
        StartCoroutine(FadeCanvasGroup(0, 1)); // Fade from transparent to opaque
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        canvasGroup.alpha = endAlpha;
    }
}
