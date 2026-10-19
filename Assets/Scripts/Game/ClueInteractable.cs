using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Press E to examine an object and record its clue in the journal. Can optionally set the next objective.
/// </summary>
public class ClueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] ClueDefinition clue;
    [Tooltip("Shown as \"Press E to examine <name>\". Leave empty for just \"Press E to examine\".")]
    [SerializeField] string displayName;
    [Tooltip("If set, becomes the current objective the first time this clue is found.")]
    [SerializeField] string objectiveOnDiscover;
    [SerializeField] UnityEvent onDiscovered;

    public ClueDefinition Clue
    {
        get => clue;
        set => clue = value;
    }

    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    public string ObjectiveOnDiscover
    {
        get => objectiveOnDiscover;
        set => objectiveOnDiscover = value;
    }

    public bool CanInteract(InteractionContext context)
    {
        return clue != null && isActiveAndEnabled;
    }

    public string GetPrompt(InteractionContext context)
    {
        return string.IsNullOrEmpty(displayName) ? "Press E to examine" : "Press E to examine " + displayName;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context))
        {
            return;
        }

        ClueJournal journal = context.Player != null ? context.Player.GetComponent<ClueJournal>() : null;
        if (journal == null)
        {
            journal = FindFirstObjectByType<ClueJournal>();
        }

        if (journal == null)
        {
            Debug.LogWarning("[ClueInteractable] No ClueJournal in the scene.", this);
            return;
        }

        if (!journal.Discover(clue))
        {
            InteractionHUD.Toast("Already in your journal (J)");
            return;
        }

        if (!string.IsNullOrEmpty(objectiveOnDiscover))
        {
            ObjectiveTracker tracker = context.Player != null ? context.Player.GetComponent<ObjectiveTracker>() : null;
            if (tracker == null)
            {
                tracker = FindFirstObjectByType<ObjectiveTracker>();
            }

            if (tracker != null)
            {
                tracker.SetObjective(objectiveOnDiscover);
            }
        }

        onDiscovered?.Invoke();
    }
}
