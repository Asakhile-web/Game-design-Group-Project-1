using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks named puzzle ids. When every required id is solved, unlocks the exit.
/// Call MarkSolved from keypad / combination / sequence scripts.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [SerializeField] string[] requiredIds = { "keypad", "combination", "sequence" };
    [SerializeField] LockedOpenable exitDoor;
    [SerializeField] string objectiveWhenComplete = "The exit is unlocked — leave the study";
    [SerializeField] bool winOnExitOpen = true;

    static PuzzleManager instance;
    readonly HashSet<string> solved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static PuzzleManager Instance => instance;
    public IReadOnlyCollection<string> SolvedIds => solved;
    public LockedOpenable ExitDoor
    {
        get => exitDoor;
        set => exitDoor = value;
    }

    public event Action<string> PuzzleSolved;
    public event Action AllRequiredSolved;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool IsSolved(string id)
    {
        return !string.IsNullOrEmpty(id) && solved.Contains(id);
    }

    public bool AreAllRequiredSolved()
    {
        if (requiredIds == null || requiredIds.Length == 0)
        {
            return solved.Count > 0;
        }

        for (int i = 0; i < requiredIds.Length; i++)
        {
            if (string.IsNullOrEmpty(requiredIds[i]) || !solved.Contains(requiredIds[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int RequiredCount => requiredIds != null ? requiredIds.Length : 0;

    public int SolvedRequiredCount
    {
        get
        {
            if (requiredIds == null)
            {
                return 0;
            }

            int n = 0;
            for (int i = 0; i < requiredIds.Length; i++)
            {
                if (IsSolved(requiredIds[i]))
                {
                    n++;
                }
            }

            return n;
        }
    }

    public void MarkSolved(string id)
    {
        if (string.IsNullOrEmpty(id) || solved.Contains(id))
        {
            return;
        }

        solved.Add(id);
        PuzzleSolved?.Invoke(id);
        AudioManager.Play(AudioCue.PuzzleSolved);
        CameraImpulse.Shake(0.12f, 0.2f);
        VolumeDirector.Pulse(0.35f);
        InteractionHUD.Toast("Puzzle solved");
        Debug.Log("[PuzzleManager] Solved '" + id + "' (" + SolvedRequiredCount + "/" + RequiredCount + ")", this);

        if (!AreAllRequiredSolved())
        {
            return;
        }

        if (exitDoor != null)
        {
            exitDoor.Unlock(openAfter: false);
        }

        ObjectiveTracker tracker = FindFirstObjectByType<ObjectiveTracker>();
        if (tracker != null && !string.IsNullOrEmpty(objectiveWhenComplete))
        {
            tracker.SetObjective(objectiveWhenComplete);
        }

        AllRequiredSolved?.Invoke();

        if (winOnExitOpen && exitDoor == null && GameFlow.Instance != null)
        {
            GameFlow.Instance.Win();
        }
    }

    /// <summary>Hook this from the exit door's onOpened UnityEvent, or call from LockedOpenable via code.</summary>
    public void NotifyExitUsed()
    {
        if (AreAllRequiredSolved() && GameFlow.Instance != null)
        {
            GameFlow.Instance.Win();
        }
    }
}
