using UnityEngine;
using TMPro;

public class AutoScrollText : MonoBehaviour
{
    public TextMeshProUGUI textComponent; // Assign TextMeshPro object in Inspector
    public float scrollSpeed = 30f;       // Speed of scrolling

    private RectTransform textRect;
    private RectTransform parentRect;

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
        if (gameObject.activeSelf && textRect != null && parentRect != null)
        {
            // Scroll the text upwards
            textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            float textHeight = textComponent.preferredHeight;
            float parentHeight = parentRect.rect.height;

            // Reset the text position when it scrolls out of view
            if (textRect.anchoredPosition.y >= textHeight)
            {
                textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, -parentHeight);
            }
        }
    }
}
