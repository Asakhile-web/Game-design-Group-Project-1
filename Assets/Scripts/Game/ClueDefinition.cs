using UnityEngine;

/// <summary>
/// One journal entry. Create via Assets &gt; Create &gt; Writers Study &gt; Clue Definition.
/// Greybox tests may also CreateRuntime these.
/// </summary>
[CreateAssetMenu(menuName = "Writers Study/Clue Definition", fileName = "ClueDefinition")]
public class ClueDefinition : ScriptableObject
{
    [SerializeField] string id = "clue";
    [SerializeField] string title = "Clue";
    [TextArea(3, 10)] [SerializeField] string description;

    public string Id => id;
    public string Title => title;
    public string Description => description;

    public bool Matches(ClueDefinition other)
    {
        if (other == null)
        {
            return false;
        }

        if (this == other)
        {
            return true;
        }

        return !string.IsNullOrEmpty(id) && id == other.id;
    }

    public static ClueDefinition CreateRuntime(string clueId, string clueTitle, string clueDescription)
    {
        ClueDefinition def = CreateInstance<ClueDefinition>();
        def.hideFlags = HideFlags.HideAndDontSave;
        def.name = clueTitle;
        def.id = clueId;
        def.title = clueTitle;
        def.description = clueDescription;
        return def;
    }
}
