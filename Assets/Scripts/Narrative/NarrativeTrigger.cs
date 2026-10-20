using UnityEngine;

/// <summary>
/// Fire-once volume. Put a BoxCollider (Is Trigger) on this object. Player CharacterController will hit it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NarrativeTrigger : MonoBehaviour
{
    [SerializeField] DialogueLine line;
    [SerializeField] string speaker = "You";
    [TextArea] [SerializeField] string fallbackText;
    [SerializeField] float duration = 4f;
    [SerializeField] bool once = true;

    bool fired;

    public DialogueLine Line
    {
        get => line;
        set => line = value;
    }

    public string FallbackText
    {
        get => fallbackText;
        set => fallbackText = value;
    }

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (fired && once)
        {
            return;
        }

        if (other.GetComponent<PlayerController>() == null && other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        fired = true;
        if (line != null)
        {
            SubtitleDirector.Play(line);
            return;
        }

        if (!string.IsNullOrEmpty(fallbackText))
        {
            SubtitleDirector.Play(speaker, fallbackText, duration);
        }
    }
}
