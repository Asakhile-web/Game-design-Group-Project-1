using UnityEngine;

/// <summary>
/// Door, drawer, or locker lid: optional key, then a local rotate or slide. No Animator.
/// Put this on the moving transform (hinge empty for doors, the drawer cube itself for slides).
/// </summary>
public class LockedOpenable : MonoBehaviour, IInteractable, IItemUseTarget
{
    public enum OpenMotion
    {
        RotateYaw,
        RotatePitch,
        SlideLocal
    }

    [Header("Lock")]
    [SerializeField] bool locked = true;
    [SerializeField] ItemDefinition requiredItem;
    [SerializeField] bool consumeRequiredItem = true;
    [SerializeField] string closedVerb = "open";
    [SerializeField] string openVerb = "close";

    [Header("Motion")]
    [SerializeField] OpenMotion motion = OpenMotion.RotateYaw;
    [SerializeField] float openAngle = 85f;
    [SerializeField] Vector3 slideOffset = new Vector3(0f, 0f, 0.38f);
    [SerializeField] float openDuration = 0.45f;
    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Vector3 closedLocalPosition;
    Quaternion closedLocalRotation;
    bool isOpen;
    float openAmount;

    public bool IsLocked => locked;
    public bool IsOpen => isOpen;

    public ItemDefinition RequiredItem
    {
        get => requiredItem;
        set => requiredItem = value;
    }

    public bool Locked
    {
        get => locked;
        set => locked = value;
    }

    public OpenMotion Motion
    {
        get => motion;
        set => motion = value;
    }

    public float OpenAngle
    {
        get => openAngle;
        set => openAngle = value;
    }

    public Vector3 SlideOffset
    {
        get => slideOffset;
        set => slideOffset = value;
    }

    void Awake()
    {
        closedLocalPosition = transform.localPosition;
        closedLocalRotation = transform.localRotation;
    }

    void Update()
    {
        float target = isOpen ? 1f : 0f;
        if (Mathf.Approximately(openAmount, target))
        {
            return;
        }

        float speed = openDuration > 0.01f ? 1f / openDuration : 20f;
        openAmount = Mathf.MoveTowards(openAmount, target, speed * Time.deltaTime);
        ApplyPose(curve.Evaluate(openAmount));
    }

    public bool CanInteract(InteractionContext context)
    {
        return isActiveAndEnabled;
    }

    public string GetPrompt(InteractionContext context)
    {
        if (locked)
        {
            string need = requiredItem != null ? requiredItem.DisplayName : "a key";
            if (requiredItem != null && context.Inventory != null && context.Inventory.Has(requiredItem))
            {
                return "Press E to unlock with " + need;
            }

            return "Locked — need " + need;
        }

        return isOpen ? "Press E to " + openVerb : "Press E to " + closedVerb;
    }

    public void Interact(InteractionContext context)
    {
        if (locked)
        {
            if (requiredItem != null && context.Inventory != null && context.Inventory.Has(requiredItem))
            {
                TryUnlockWith(requiredItem, context, openAfter: true);
                return;
            }

            string need = requiredItem != null ? requiredItem.DisplayName : "a key";
            InteractionHUD.Toast("Locked — need " + need);
            return;
        }

        Toggle();
    }

    public bool CanUseItem(ItemDefinition item, InteractionContext context)
    {
        return locked && item != null && requiredItem != null && item.Matches(requiredItem);
    }

    public string GetUsePrompt(ItemDefinition item, InteractionContext context)
    {
        if (CanUseItem(item, context))
        {
            return "Press F to use " + item.DisplayName;
        }

        if (locked && item != null && requiredItem != null)
        {
            return "Wrong item (need " + requiredItem.DisplayName + ")";
        }

        return string.Empty;
    }

    public bool TryUseItem(ItemDefinition item, InteractionContext context)
    {
        if (!CanUseItem(item, context))
        {
            if (locked && requiredItem != null)
            {
                InteractionHUD.Toast("Need " + requiredItem.DisplayName);
            }

            return false;
        }

        return TryUnlockWith(item, context, openAfter: true);
    }

    public void Unlock(bool openAfter)
    {
        locked = false;
        if (openAfter && !isOpen)
        {
            isOpen = true;
        }
    }

    public void Toggle()
    {
        isOpen = !isOpen;
    }

    bool TryUnlockWith(ItemDefinition item, InteractionContext context, bool openAfter)
    {
        if (consumeRequiredItem && item.ConsumeOnUse && context.Inventory != null)
        {
            context.Inventory.TryConsume(item);
        }

        Unlock(openAfter);
        InteractionHUD.Toast("Unlocked");
        Debug.Log("[LockedOpenable] Unlocked " + name, this);
        return true;
    }

    void ApplyPose(float t)
    {
        if (motion == OpenMotion.SlideLocal)
        {
            transform.localPosition = closedLocalPosition + slideOffset * t;
            transform.localRotation = closedLocalRotation;
            return;
        }

        Vector3 axis = motion == OpenMotion.RotatePitch ? Vector3.right : Vector3.up;
        transform.localRotation = closedLocalRotation * Quaternion.AngleAxis(openAngle * t, axis);
        transform.localPosition = closedLocalPosition;
    }
}
