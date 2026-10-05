using UnityEngine;

/// <summary>
/// Data for one inventory item. Create via Assets &gt; Create &gt; Writers Study &gt; Item Definition.
/// Greybox tests may also CreateInstance these at runtime.
/// </summary>
[CreateAssetMenu(menuName = "Writers Study/Item Definition", fileName = "ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] string id = "item";
    [SerializeField] string displayName = "Item";
    [SerializeField] Sprite icon;
    [TextArea] [SerializeField] string description;
    [SerializeField] bool consumeOnUse = true;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;
    public bool ConsumeOnUse => consumeOnUse;

    public bool Matches(ItemDefinition other)
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

    public static ItemDefinition CreateRuntime(string itemId, string itemDisplayName, bool consume = true)
    {
        ItemDefinition def = CreateInstance<ItemDefinition>();
        def.hideFlags = HideFlags.HideAndDontSave;
        def.name = itemDisplayName;
        def.id = itemId;
        def.displayName = itemDisplayName;
        def.consumeOnUse = consume;
        return def;
    }
}
