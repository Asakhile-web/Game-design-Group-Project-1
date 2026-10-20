using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Accepts a specific selected item (F / click). Can unlock a LockedOpenable and/or fire a UnityEvent.
/// Locked drawers also implement IItemUseTarget themselves; use this for padlocks, slots, and one-shot uses.
/// </summary>
public class ItemUseTarget : MonoBehaviour, IItemUseTarget, IInteractable
{
    [SerializeField] ItemDefinition acceptedItem;
    [SerializeField] bool consumeIfItemAllows = true;
    [SerializeField] LockedOpenable unlocks;
    [SerializeField] bool hideAfterSuccess;
    [SerializeField] UnityEvent onUsed;
    [SerializeField] string interactVerb = "inspect";

    bool used;

    public ItemDefinition AcceptedItem
    {
        get => acceptedItem;
        set => acceptedItem = value;
    }

    public LockedOpenable Unlocks
    {
        get => unlocks;
        set => unlocks = value;
    }

    public bool CanInteract(InteractionContext context)
    {
        return !used && isActiveAndEnabled;
    }

    public string GetPrompt(InteractionContext context)
    {
        if (acceptedItem != null && context.Inventory != null && context.Inventory.Has(acceptedItem))
        {
            return "Press E or F to use " + acceptedItem.DisplayName;
        }

        if (acceptedItem != null)
        {
            return "Needs " + acceptedItem.DisplayName;
        }

        return string.IsNullOrEmpty(interactVerb) ? "Press E" : "Press E to " + interactVerb;
    }

    public void Interact(InteractionContext context)
    {
        if (acceptedItem == null || context.Inventory == null)
        {
            return;
        }

        if (!context.Inventory.Has(acceptedItem))
        {
            InteractionHUD.Toast("Need " + acceptedItem.DisplayName);
            return;
        }

        ItemDefinition toUse = context.Inventory.SelectedItem != null && context.Inventory.SelectedItem.Matches(acceptedItem)
            ? context.Inventory.SelectedItem
            : acceptedItem;
        TryUseItem(toUse, context);
    }

    public bool CanUseItem(ItemDefinition item, InteractionContext context)
    {
        return !used && item != null && acceptedItem != null && item.Matches(acceptedItem);
    }

    public string GetUsePrompt(ItemDefinition item, InteractionContext context)
    {
        if (CanUseItem(item, context))
        {
            return "Press F to use " + item.DisplayName;
        }

        if (item != null && acceptedItem != null)
        {
            return "That does not fit (need " + acceptedItem.DisplayName + ")";
        }

        return string.Empty;
    }

    public bool TryUseItem(ItemDefinition item, InteractionContext context)
    {
        if (!CanUseItem(item, context))
        {
            if (item != null && acceptedItem != null)
            {
                InteractionHUD.Toast("Need " + acceptedItem.DisplayName);
            }

            return false;
        }

        if (consumeIfItemAllows && item.ConsumeOnUse)
        {
            context.Inventory.TryConsume(item);
        }

        used = true;
        if (unlocks != null)
        {
            unlocks.Unlock(openAfter: true);
        }

        onUsed?.Invoke();
        InteractionHUD.Toast("Used " + item.DisplayName);

        if (hideAfterSuccess)
        {
            gameObject.SetActive(false);
        }

        return true;
    }
}
