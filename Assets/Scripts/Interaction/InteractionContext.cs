using UnityEngine;

/// <summary>
/// Data passed into interactables so they do not need to Find the player.
/// </summary>
public readonly struct InteractionContext
{
    public readonly PlayerController Player;
    public readonly Transform CameraTransform;
    public readonly Transform HoldPoint;
    public readonly InventorySystem Inventory;

    public InteractionContext(PlayerController player, Transform cameraTransform, Transform holdPoint, InventorySystem inventory)
    {
        Player = player;
        CameraTransform = cameraTransform;
        HoldPoint = holdPoint;
        Inventory = inventory;
    }
}
