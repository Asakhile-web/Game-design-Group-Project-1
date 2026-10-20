using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Three rotating dials (0-9). Press E, set each dial, then Submit.
/// </summary>
public class CombinationLock : MonoBehaviour, IInteractable, IEscapableOverlay
{
    [SerializeField] string puzzleId = "combination";
    [SerializeField] int[] combination = { 3, 1, 4 };
    [SerializeField] LockedOpenable unlocks;
    [SerializeField] string promptAction = "turn the dials";

    readonly int[] dials = { 0, 0, 0 };
    bool open;
    bool solved;
    RectTransform overlay;
    TextMeshProUGUI[] dialLabels;

    public bool CanInteract(InteractionContext context)
    {
        return isActiveAndEnabled && !solved;
    }

    public string GetPrompt(InteractionContext context)
    {
        return solved ? string.Empty : "Press E to " + promptAction;
    }

    public void Interact(InteractionContext context)
    {
        if (!CanInteract(context) || open)
        {
            return;
        }

        OpenLock();
    }

    public void CloseFromEscape()
    {
        CloseLock();
    }

    void OpenLock()
    {
        EnsureUi();
        open = true;
        overlay.gameObject.SetActive(true);
        RefreshDials();
        GameOverlay.Push(this);
        AudioManager.Play(AudioCue.UiClick);
    }

    void CloseLock()
    {
        if (!open)
        {
            return;
        }

        open = false;
        if (overlay != null)
        {
            overlay.gameObject.SetActive(false);
        }

        GameOverlay.Pop(this);
    }

    void Nudge(int index, int delta)
    {
        if (index < 0 || index >= dials.Length)
        {
            return;
        }

        dials[index] = (dials[index] + delta + 10) % 10;
        RefreshDials();
        AudioManager.Play(AudioCue.UiClick);
    }

    void Submit()
    {
        bool match = combination != null && combination.Length == dials.Length;
        if (match)
        {
            for (int i = 0; i < dials.Length; i++)
            {
                if (dials[i] != combination[i])
                {
                    match = false;
                    break;
                }
            }
        }

        if (!match)
        {
            InteractionHUD.Toast("The dials click, but nothing opens");
            return;
        }

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

        CloseLock();
    }

    void RefreshDials()
    {
        if (dialLabels == null)
        {
            return;
        }

        for (int i = 0; i < dialLabels.Length; i++)
        {
            dialLabels[i].text = dials[i].ToString();
        }
    }

    void EnsureUi()
    {
        if (overlay != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("CombinationCanvas", typeof(RectTransform));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 260;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        Image dim = UiKit.CreateImage("Dim", canvasObject.transform, UiKit.Overlay, false, true);
        overlay = dim.rectTransform;
        UiKit.Stretch(overlay);

        Image panel = UiKit.CreateImage("Panel", overlay, UiKit.Panel, true, true);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 420f));
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 12f, new RectOffset(32, 32, 28, 28));

        UiKit.CreateText("Title", panel.transform, "Combination", 32f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.CreateText("Hint", panel.transform, "Match the order of the books.", 18f, UiKit.TextDim, TextAlignmentOptions.Center);

        RectTransform row = UiKit.CreateRect("Dials", panel.transform);
        UiKit.SetLayoutHeight(row, 160f);
        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        dialLabels = new TextMeshProUGUI[dials.Length];
        for (int i = 0; i < dials.Length; i++)
        {
            dialLabels[i] = BuildDial(row, i);
        }

        UiKit.CreateButton("Submit", panel.transform, "Submit", Submit);
        UiKit.CreateButton("Close", panel.transform, "Close", CloseLock, 48f);
        overlay.gameObject.SetActive(false);
    }

    TextMeshProUGUI BuildDial(Transform parent, int index)
    {
        Image column = UiKit.CreateImage("Dial" + index, parent, UiKit.Inset, true, true);
        UiKit.AddVerticalLayout(column.gameObject, 6f, new RectOffset(8, 8, 8, 8));
        int captured = index;
        UiKit.CreateButton("Up", column.transform, "+", () => Nudge(captured, 1), 40f, 22f);
        TextMeshProUGUI value = UiKit.CreateText("Value", column.transform, "0", 42f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(value, 52f);
        UiKit.CreateButton("Down", column.transform, "−", () => Nudge(captured, -1), 40f, 22f);
        return value;
    }
}
