using UnityEngine;

/// <summary>
/// Stage 1 stand-in for inventory: parents the object to the player's hold point (same behaviour as the old tag pickup).
/// Replaced by Inventory in Stage 2.
/// </summary>
public class PickupHoldInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string itemDisplayName = "item";
    [SerializeField] bool disableColliderOnPickup = true;

    bool held;
    string cachedPrompt;

    void Awake()
    {
        cachedPrompt = "Press E to pick up " + itemDisplayName;
    }

    public bool CanInteract(InteractionContext context)
    {
        return !held && isActiveAndEnabled;
    }

    public string GetPrompt(InteractionContext context)
    {
        return cachedPrompt;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
        {
            return;
        }

        if (context.HoldPoint == null)
        {
            Debug.LogWarning("[PickupHoldInteractable] No hold point on player. Create an empty child named HoldPoint.", this);
            return;
        }

        held = true;
        transform.SetParent(context.HoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (disableColliderOnPickup && col != null)
        {
            col.enabled = false;
        }

        Debug.Log("[PickupHoldInteractable] Picked up " + name, this);
    }
}
