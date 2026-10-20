using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight stack for in-world overlays (notes, keypad, combination) that steal Esc
/// before the pause menu and suspend player look/move without changing timeScale.
/// </summary>
public static class GameOverlay
{
    static readonly List<IEscapableOverlay> stack = new List<IEscapableOverlay>();

    public static bool HasOverlay => stack.Count > 0;

    public static void Push(IEscapableOverlay overlay)
    {
        if (overlay == null || stack.Contains(overlay))
        {
            return;
        }

        stack.Add(overlay);
        ApplyPlayerLock();
    }

    public static void Pop(IEscapableOverlay overlay)
    {
        if (overlay == null)
        {
            return;
        }

        stack.Remove(overlay);
        if (stack.Count == 0)
        {
            ReleasePlayerLock();
        }
    }

    /// <summary>True if Esc was consumed by an overlay.</summary>
    public static bool TryHandleEscape()
    {
        if (stack.Count == 0)
        {
            return false;
        }

        IEscapableOverlay top = stack[stack.Count - 1];
        top.CloseFromEscape();
        return true;
    }

    public static void CloseAll()
    {
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            stack[i].CloseFromEscape();
        }

        stack.Clear();
        ReleasePlayerLock();
    }

    static void ApplyPlayerLock()
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ControlEnabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    static void ReleasePlayerLock()
    {
        GameMenu menu = GameMenu.Instance;
        if (menu != null && menu.IsPaused)
        {
            return;
        }

        GameFlow flow = GameFlow.Instance;
        if (flow != null && flow.IsEnded)
        {
            return;
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ControlEnabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

public interface IEscapableOverlay
{
    void CloseFromEscape();
}
