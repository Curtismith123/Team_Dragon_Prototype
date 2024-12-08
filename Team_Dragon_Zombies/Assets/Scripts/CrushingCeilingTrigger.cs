using UnityEngine;

public class CrushingCeilingTrigger : MonoBehaviour
{
    public delegate void TriggerAction();
    public event TriggerAction OnTriggered;

    [Tooltip("Only trigger on objects with these tags.")]
    public string[] triggerTags = { "Player" };

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered)
        {
            foreach (string tag in triggerTags)
            {
                if (other.CompareTag(tag))
                {
                    hasTriggered = true;
                    OnTriggered?.Invoke();
                    break;
                }
            }
        }
    }
}
