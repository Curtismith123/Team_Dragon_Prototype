using System.Collections;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] public float fadeDuration = 1.0f;

    private bool hasFadedIn = false;




    private void Start()
    {
        
        canvasGroup.alpha = 1;
        if (!hasFadedIn)
        {
            
            FadeIn();
            hasFadedIn = true;
        }
    }


    public void FadeIn()
    {
        StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
