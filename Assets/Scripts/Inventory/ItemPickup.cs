using UnityEngine;

/// <summary>
/// World pickup that adds an ItemDefinition to InventorySystem and hides the prop.
/// </summary>
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] ItemDefinition definition;
    [SerializeField] bool disableAfterPickup = true;

    bool taken;

    public ItemDefinition Definition
    {
        get => definition;
        set => definition = value;
    }

    public bool CanInteract(InteractionContext context)
    {
        return !taken && isActiveAndEnabled && definition != null;
    }

    public string GetPrompt(InteractionContext context)
    {
        string name = definition != null ? definition.DisplayName : "item";
        return "Press E to pick up " + name;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
        {
            return;
        }

        InventorySystem inventory = context.Inventory;
        if (inventory == null)
        {
            Debug.LogWarning("[ItemPickup] Player has no InventorySystem.", this);
            return;
        }

        if (!inventory.TryAdd(definition))
        {
            return;
        }

        taken = true;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (disableAfterPickup)
        {
            gameObject.SetActive(false);
        }
    }
}
