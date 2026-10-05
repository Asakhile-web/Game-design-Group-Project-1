/// <summary>
/// Anything the player can use with E (doors, notes, pickups, keypads).
/// </summary>
public interface IInteractable
{
    bool CanInteract(InteractionContext context);
    string GetPrompt(InteractionContext context);
    void Interact(InteractionContext context);
}
