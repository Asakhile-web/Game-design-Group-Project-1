
using UnityEngine;

/// <summary>
/// Camera-forward raycast that finds IInteractable and IItemUseTarget.
/// Displays E and F interaction prompts.
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

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

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

        if (hit.collider != cachedCollider)
        {
            cachedCollider = hit.collider;
            Debug.Log("RAYCAST HIT: " + hit.collider.name);
        }

        cachedInteractable =
            hit.collider.GetComponentInParent<IInteractable>();

        cachedItemUseTarget =
            hit.collider.GetComponentInParent<IItemUseTarget>();

        // E interaction prompt.
        if (cachedInteractable != null)
        {
            if (cachedInteractable.CanInteract(context))
            {
                cachedPrompt = "Press E to interact";
                HasTarget = true;

                Debug.Log("INTERACTABLE DETECTED: Press E to interact");
            }
            else
            {
                Debug.Log("INTERACTABLE FOUND BUT CANNOT INTERACT");
                cachedInteractable = null;
            }
        }

        // F item-use prompt.
        if (cachedItemUseTarget != null &&
            context.Inventory != null)
        {
            ItemDefinition selectedItem =
                context.Inventory.SelectedItem;

            if (selectedItem != null &&
                cachedItemUseTarget.CanUseItem(selectedItem, context))
            {
                CurrentUsePrompt = "Press F to use";
                HasTarget = true;

                Debug.Log("ITEM USE DETECTED: Press F to use");
            }
        }

        if (!HasTarget)
        {
            cachedPrompt = "";
            CurrentUsePrompt = "";
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

        if (!cachedItemUseTarget.CanUseItem(selectedItem, context))
            return false;

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
