/// <summary>
/// Looked-at object that accepts the selected hotbar item (F / left mouse).
/// </summary>
public interface IItemUseTarget
{
    bool CanUseItem(ItemDefinition item, InteractionContext context);
    string GetUsePrompt(ItemDefinition item, InteractionContext context);
    bool TryUseItem(ItemDefinition item, InteractionContext context);
}
