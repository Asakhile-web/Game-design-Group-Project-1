using UnityEngine;

/// <summary>
/// Place on the far side of the exit. When the player walks through after all puzzles,
/// GameFlow wins. Also works as IInteractable on the door itself.
/// </summary>
public class ExitTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] string lockedPrompt = "The exit is still locked";
    [SerializeField] string openPrompt = "Press E to leave";
    [SerializeField] bool winOnTriggerEnter = true;

    public bool CanInteract(InteractionContext context)
    {
        return isActiveAndEnabled;
    }

    public string GetPrompt(InteractionContext context)
    {
        PuzzleManager puzzles = PuzzleManager.Instance;
        if (puzzles != null && puzzles.AreAllRequiredSolved())
        {
            return openPrompt;
        }

        return lockedPrompt;
    }

    public void Interact(InteractionContext context)
    {
        TryWin();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!winOnTriggerEnter)
        {
            return;
        }

        if (other.GetComponent<PlayerController>() == null && other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        TryWin();
    }

    void TryWin()
    {
        PuzzleManager puzzles = PuzzleManager.Instance;
        if (puzzles == null || !puzzles.AreAllRequiredSolved())
        {
            InteractionHUD.Toast("Something still holds the door");
            return;
        }

        puzzles.NotifyExitUsed();
    }
}
