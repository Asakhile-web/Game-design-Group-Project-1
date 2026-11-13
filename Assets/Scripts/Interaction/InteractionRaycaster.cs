
using UnityEngine;

/// <summary>
/// Camera-forward raycast that finds IInteractable and IItemUseTarget.
/// Supports ordinary interactions and selected inventory item usage.
/// </summary>
public class InteractionRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float range = 3f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction =
        QueryTriggerInteraction.Ignore;

    private IInteractable cachedInteractable;
    private IItemUseTarget cachedItemUseTarget;
    private Collider cachedCollider;

    private string cachedPrompt = "";

    public bool HasTarget { get; private set; }

    public Collider CurrentCollider => cachedCollider;

    public string CurrentPrompt => cachedPrompt;

    public string CurrentUsePrompt { get; private set; } = "";

    public Transform RayOrigin
    {
        get => rayOrigin;
        set => rayOrigin = value;
    }

    public float Range
    {
        get => range;
        set => range = value;
    }

    public void Tick(InteractionContext context)
    {
        HasTarget = false;
        cachedPrompt = "";
        CurrentUsePrompt = "";

        cachedInteractable = null;
        cachedItemUseTarget = null;

        if (rayOrigin == null)
        {
            cachedCollider = null;
            return;
        }

        Ray ray = new Ray(
            rayOrigin.position,
            rayOrigin.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            range,
            hitMask,
            triggerInteraction))
        {
            cachedCollider = null;
            return;
        }

        // Debug: show which object the camera is hitting.
        if (hit.collider != cachedCollider)
        {
            cachedCollider = hit.collider;
            Debug.Log("RAYCAST HIT: " + hit.collider.name);
        }

        // Find interaction components on the hit object or its parents.
        cachedInteractable =
            hit.collider.GetComponentInParent<IInteractable>();

        cachedItemUseTarget =
            hit.collider.GetComponentInParent<IItemUseTarget>();

        // Check ordinary interaction.
        if (cachedInteractable != null &&
            cachedInteractable.CanInteract(context))
        {
            cachedPrompt = cachedInteractable.GetPrompt(context);
            HasTarget = true;
        }
        else
        {
            cachedInteractable = null;
        }

        // Check selected inventory item usage.
        if (cachedItemUseTarget != null)
        {
            ItemDefinition selectedItem =
                context.Inventory != null
                    ? context.Inventory.SelectedItem
                    : null;

            if (selectedItem != null)
            {
                CurrentUsePrompt =
                    cachedItemUseTarget.GetUsePrompt(
                        selectedItem,
                        context
                    );
            }

            HasTarget = true;
        }

        if (!HasTarget)
        {
            Clear();
        }
    }

    public bool TryInteract(InteractionContext context)
    {
        if (cachedInteractable == null)
            return false;

        if (!cachedInteractable.CanInteract(context))
            return false;

        cachedInteractable.Interact(context);

        return true;
    }

    public bool TryUseSelectedItem(InteractionContext context)
    {
        if (cachedItemUseTarget == null)
            return false;

        if (context.Inventory == null)
            return false;

        ItemDefinition selectedItem =
            context.Inventory.SelectedItem;

        if (selectedItem == null)
            return false;

        if (!cachedItemUseTarget.CanUseItem(
            selectedItem,
            context))
        {
            return false;
        }

        return cachedItemUseTarget.TryUseItem(
            selectedItem,
            context
        );
    }

    public void Clear()
    {
        HasTarget = false;

        cachedInteractable = null;
        cachedItemUseTarget = null;
        cachedCollider = null;

        cachedPrompt = "";
        CurrentUsePrompt = "";
    }
}
