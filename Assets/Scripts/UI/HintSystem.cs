using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Limited hints (H). Copy is assigned in the Inspector so designers can write without code.
/// </summary>
public class HintSystem : MonoBehaviour
{
    [System.Serializable]
    public struct HintEntry
    {
        public string puzzleId;
        [TextArea] public string text;
    }

    [SerializeField] HintEntry[] hints;
    [SerializeField] int maxUses = 3;
    [SerializeField] float cooldownSeconds = 20f;

    InputAction hintAction;
    int uses;
    float nextAllowed;

    public int RemainingUses => Mathf.Max(0, maxUses - uses);

    public void Configure(HintEntry[] entries, int usesAllowed = 3, float cooldown = 20f)
    {
        if (entries != null)
        {
            hints = entries;
        }

        maxUses = usesAllowed;
        cooldownSeconds = cooldown;
    }

    void Awake()
    {
        hintAction = new InputAction("Hint", InputActionType.Button);
        hintAction.AddBinding("<Keyboard>/h");
        hintAction.performed += _ => TryShowHint();
    }

    void OnEnable()
    {
        hintAction.Enable();
    }

    void OnDisable()
    {
        hintAction.Disable();
    }

    void OnDestroy()
    {
        hintAction?.Dispose();
    }

    public void TryShowHint()
    {
        if (GameFlow.Instance != null && GameFlow.Instance.IsEnded)
        {
            return;
        }

        if (GameMenu.Instance != null && GameMenu.Instance.IsPaused)
        {
            return;
        }

        if (GameOverlay.HasOverlay)
        {
            return;
        }

        if (uses >= maxUses)
        {
            InteractionHUD.Toast("No hints left");
            return;
        }

        if (Time.unscaledTime < nextAllowed)
        {
            int wait = Mathf.CeilToInt(nextAllowed - Time.unscaledTime);
            InteractionHUD.Toast("Hint available in " + wait + "s");
            return;
        }

        string text = ResolveHint();
        if (string.IsNullOrEmpty(text))
        {
            InteractionHUD.Toast("Nothing more to hint at");
            return;
        }

        uses++;
        nextAllowed = Time.unscaledTime + cooldownSeconds;
        AudioManager.Play(AudioCue.Hint);
        InteractionHUD.Toast(text, 4.5f);
        GameHUD hud = FindFirstObjectByType<GameHUD>();
        if (hud != null)
        {
            hud.ShowNotification("Hint (" + RemainingUses + " left)", text);
        }
    }

    string ResolveHint()
    {
        PuzzleManager puzzles = PuzzleManager.Instance;
        if (hints == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < hints.Length; i++)
        {
            HintEntry entry = hints[i];
            if (string.IsNullOrEmpty(entry.text))
            {
                continue;
            }

            if (puzzles != null && !string.IsNullOrEmpty(entry.puzzleId) && puzzles.IsSolved(entry.puzzleId))
            {
                continue;
            }

            return entry.text;
        }

        return "Search every drawer. Keys first, then codes.";
    }
}
