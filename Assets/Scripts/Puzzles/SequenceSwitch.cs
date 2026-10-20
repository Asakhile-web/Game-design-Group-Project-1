using UnityEngine;

/// <summary>
/// One step in a SequencePuzzle. Lights up when pressed in the correct order.
/// </summary>
public class SequenceSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] string promptAction = "press";
    [SerializeField] Color unlit = new Color(0.15f, 0.12f, 0.1f);
    [SerializeField] Color lit = new Color(0.95f, 0.72f, 0.28f);

    SequencePuzzle puzzle;
    int index;
    bool isLit;
    Renderer cachedRenderer;
    MaterialPropertyBlock block;

    public void Bind(SequencePuzzle owner, int switchIndex)
    {
        puzzle = owner;
        index = switchIndex;
    }

    void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        SetLit(false);
    }

    public bool CanInteract(InteractionContext context)
    {
        return isActiveAndEnabled && puzzle != null && !puzzle.IsSolved;
    }

    public string GetPrompt(InteractionContext context)
    {
        return "Press E to " + promptAction;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
        {
            return;
        }

        SetLit(true);
        puzzle.NotifyPressed(index);
    }

    public void SetLit(bool value)
    {
        isLit = value;
        ApplyColor(value ? lit : unlit);
    }

    void ApplyColor(Color color)
    {
        if (cachedRenderer == null)
        {
            return;
        }

        cachedRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        cachedRenderer.SetPropertyBlock(block);
    }
}
