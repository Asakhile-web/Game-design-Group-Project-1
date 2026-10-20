using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic E-to-use object. Use for prototypes, switches, and Inspector-wired events.
/// </summary>
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string promptAction = "inspect";
    [SerializeField] bool canInteract = true;
    [SerializeField] UnityEvent onInteract;

    string cachedPrompt;

    void Awake()
    {
        cachedPrompt = string.IsNullOrEmpty(promptAction) ? "Press E" : "Press E to " + promptAction;
    }

    public bool CanInteract(InteractionContext context)
    {
        return canInteract && isActiveAndEnabled;
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

        onInteract?.Invoke();
        Debug.Log("[SimpleInteractable] Used " + name, this);
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}
