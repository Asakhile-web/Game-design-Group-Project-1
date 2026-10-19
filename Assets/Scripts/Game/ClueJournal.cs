using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clues the player has discovered, in discovery order. Attach to the player (PlayerController adds one if missing).
/// </summary>
public class ClueJournal : MonoBehaviour
{
    readonly List<ClueDefinition> clues = new List<ClueDefinition>();

    public IReadOnlyList<ClueDefinition> Clues => clues;
    public int Count => clues.Count;

    public event Action<ClueDefinition> ClueAdded;

    public bool Has(ClueDefinition clue)
    {
        if (clue == null)
        {
            return false;
        }

        for (int i = 0; i < clues.Count; i++)
        {
            if (clues[i].Matches(clue))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Adds the clue if it is new. Returns false if it was already recorded.</summary>
    public bool Discover(ClueDefinition clue)
    {
        if (clue == null || Has(clue))
        {
            return false;
        }

        clues.Add(clue);
        ClueAdded?.Invoke(clue);
        Debug.Log("[ClueJournal] Recorded clue: " + clue.Title, this);
        return true;
    }
}
