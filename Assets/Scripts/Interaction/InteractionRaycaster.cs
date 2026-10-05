using UnityEngine;

/// <summary>
/// Camera-forward raycast that finds IInteractable and IItemUseTarget.
/// Caches the last collider so GetComponent is not called every frame.
/// </summary>
public class InteractionRaycaster : MonoBehaviour
{
    [SerializeField] Transform rayOrigin;
    [SerializeField] float range = 3f;
    [SerializeField] LayerMask hitMask = ~0;
    [SerializeField] QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    Collider cachedCollider;
    IInteractable cachedInteractable;
    IItemUseTarget cachedUseTarget;
    string cachedPrompt = string.Empty;
    string cachedUsePrompt = string.Empty;
    bool hasTarget;

    public bool HasTarget => hasTarget;
    public IInteractable Current => hasTarget ? cachedInteractable : null;
    public IItemUseTarget CurrentUseTarget => hasTarget ? cachedUseTarget : null;
    public string CurrentPrompt => hasTarget ? cachedPrompt : string.Empty;
    public string CurrentUsePrompt => hasTarget ? cachedUsePrompt : string.Empty;
    public Collider CurrentCollider => hasTarget ? cachedCollider : null;

    public float Range
    {
        get => range;
        set => range = value;
    }

    public Transform RayOrigin
    {
        get => rayOrigin;
        set => rayOrigin = value;
    }

    /// <summary>
    /// Call from PlayerController each frame. Do not enable a second Update raycast on this component.
    /// </summary>
    public void Tick(InteractionContext context)
    {
        if (rayOrigin == null)
        {
            hasTarget = false;
            return;
        }

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, triggerInteraction))
        {
            Clear();
            return;
        }

        if (hit.collider != cachedCollider)
        {
            cachedCollider = hit.collider;
            cachedInteractable = ResolveInteractable(hit.collider);
            cachedUseTarget = ResolveUseTarget(hit.collider);
        }

        bool interactOk = cachedInteractable != null && cachedInteractable.CanInteract(context);
        bool useOk = cachedUseTarget != null;

        if (!interactOk && !useOk)
        {
            Clear();
            return;
        }

        hasTarget = true;
        cachedPrompt = interactOk ? cachedInteractable.GetPrompt(context) : string.Empty;

        ItemDefinition selected = context.Inventory != null ? context.Inventory.SelectedItem : null;
        cachedUsePrompt = string.Empty;
        if (cachedUseTarget != null && selected != null)
        {
            cachedUsePrompt = cachedUseTarget.GetUsePrompt(selected, context);
        }
    }

    public bool TryInteract(InteractionContext context)
    {
        Tick(context);
        if (!hasTarget || cachedInteractable == null || !cachedInteractable.CanInteract(context))
        {
            return false;
        }

        cachedInteractable.Interact(context);
        return true;
    }

    public bool TryUseSelectedItem(InteractionContext context)
    {
        Tick(context);
        InventorySystem inventory = context.Inventory;
        if (inventory == null)
        {
            Debug.LogWarning("[InteractionRaycaster] No InventorySystem on player.");
            return false;
        }

        ItemDefinition selected = inventory.SelectedItem;
        if (selected == null)
        {
            return false;
        }

        if (!hasTarget || cachedUseTarget == null)
        {
            return false;
        }

        return cachedUseTarget.TryUseItem(selected, context);
    }

    static IInteractable ResolveInteractable(Collider col)
    {
        IInteractable interactable = col.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            return interactable;
        }

        if (col.CompareTag("Pickup"))
        {
            ItemPickup itemPickup = col.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                return itemPickup;
            }

            PickupHoldInteractable pickup = col.GetComponent<PickupHoldInteractable>();
            if (pickup == null)
            {
                pickup = col.gameObject.AddComponent<PickupHoldInteractable>();
            }

            return pickup;
        }

        return null;
    }

    static IItemUseTarget ResolveUseTarget(Collider col)
    {
        return col.GetComponentInParent<IItemUseTarget>();
    }

    void Clear()
    {
        cachedCollider = null;
        cachedInteractable = null;
        cachedUseTarget = null;
        cachedPrompt = string.Empty;
        cachedUsePrompt = string.Empty;
        hasTarget = false;
    }
}
