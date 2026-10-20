using UnityEngine;

/// <summary>One subtitle / inner-thought line. Create via Assets &gt; Create &gt; Writers Study &gt; Dialogue Line.</summary>
[CreateAssetMenu(menuName = "Writers Study/Dialogue Line", fileName = "DialogueLine")]
public class DialogueLine : ScriptableObject
{
    [SerializeField] string speaker = "You";
    [TextArea(2, 6)] [SerializeField] string text = "...";
    [SerializeField] float duration = 3.5f;

    public string Speaker => speaker;
    public string Text => text;
    public float Duration => duration;

    public static DialogueLine CreateRuntime(string speakerName, string body, float seconds = 3.5f)
    {
        DialogueLine line = CreateInstance<DialogueLine>();
        line.hideFlags = HideFlags.HideAndDontSave;
        line.name = speakerName;
        line.speaker = speakerName;
        line.text = body;
        line.duration = seconds;
        return line;
    }
}
