using UnityEngine;

/// <summary>
/// Ordered switch puzzle. SequenceSwitch children (or assigned list) must be used in index order.
/// </summary>
public class SequencePuzzle : MonoBehaviour
{
    [SerializeField] string puzzleId = "sequence";
    [SerializeField] SequenceSwitch[] switches;
    [SerializeField] LockedOpenable unlocks;
    [SerializeField] bool resetOnMistake = true;

    int expected;
    bool solved;

    public bool IsSolved => solved;

    public void SetSwitches(SequenceSwitch[] list)
    {
        switches = list;
        if (switches == null)
        {
            return;
        }

        for (int i = 0; i < switches.Length; i++)
        {
            if (switches[i] != null)
            {
                switches[i].Bind(this, i);
            }
        }
    }

    void Awake()
    {
        if (switches == null)
        {
            switches = GetComponentsInChildren<SequenceSwitch>(true);
        }

        for (int i = 0; i < switches.Length; i++)
        {
            if (switches[i] != null)
            {
                switches[i].Bind(this, i);
            }
        }
    }

    public void NotifyPressed(int index)
    {
        if (solved)
        {
            return;
        }

        if (index == expected)
        {
            expected++;
            AudioManager.Play(AudioCue.UiClick);
            if (switches != null && expected >= switches.Length)
            {
                Complete();
            }

            return;
        }

        expected = 0;
        if (resetOnMistake)
        {
            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] != null)
                {
                    switches[i].SetLit(false);
                }
            }
        }

        InteractionHUD.Toast("Wrong order — try again");
    }

    void Complete()
    {
        solved = true;
        if (unlocks != null)
        {
            unlocks.Unlock(openAfter: true);
        }

        PuzzleManager manager = PuzzleManager.Instance;
        if (manager != null)
        {
            manager.MarkSolved(puzzleId);
        }
    }
}
