using UnityEngine;
using TMPro;

public class AutoScrollText : MonoBehaviour
{
    public TextMeshProUGUI textComponent; // Assign TextMeshPro object in Inspector
    public float scrollSpeed = 30f;       // Speed of scrolling

    private RectTransform textRect;
    private RectTransform parentRect;
    private bool isScrolling = false;

    void Start()
    {
        if (textComponent != null)
        {
            textRect = textComponent.GetComponent<RectTransform>();
            parentRect = textRect.parent.GetComponent<RectTransform>();
        }

    }

    void Update()
    {
        // Check if the GameObject is active
        if (gameObject.activeSelf && !isScrolling)
        {
            StartScrolling();
        }

        if (isScrolling && textRect != null && parentRect != null)
        {
            textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            float textHeight = textComponent.preferredHeight;
            float parentHeight = parentRect.rect.height;

            if (textRect.anchoredPosition.y >= (textHeight - parentHeight) / 2f)
            {
                // Stop scrolling when the final message is centered
                textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, (textHeight - parentHeight) / 2f);
                isScrolling = false;
            }
        }
    }

    private void StartScrolling()
    {
        ResetTextPosition();
        isScrolling = true;
    }

    private void ResetTextPosition()
    {
        // Reset to the starting position at the bottom of the parent area
        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, -parentRect.rect.height / 2f);
    }
}
