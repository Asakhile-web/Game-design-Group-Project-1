using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Digit code lock. Press E to open the pad, type 0-9, Enter to submit, Esc to close.
/// </summary>
public class KeypadLock : MonoBehaviour, IInteractable, IEscapableOverlay
{
    [SerializeField] string puzzleId = "keypad";
    [SerializeField] string code = "3141";
    [SerializeField] int maxDigits = 8;
    [SerializeField] LockedOpenable unlocks;
    [SerializeField] string promptAction = "use keypad";

    string entry = string.Empty;
    bool open;
    bool solved;
    RectTransform overlay;
    TextMeshProUGUI display;
    TextMeshProUGUI status;

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

        OpenPad();
    }

    public void CloseFromEscape()
    {
        ClosePad();
    }

    void Update()
    {
        if (!open || Keyboard.current == null)
        {
            return;
        }

        Keyboard kb = Keyboard.current;
        for (int i = 0; i <= 9; i++)
        {
            if (WasPressed(i, kb))
            {
                Append(i.ToString());
            }
        }

        if (kb.backspaceKey.wasPressedThisFrame)
        {
            if (entry.Length > 0)
            {
                entry = entry.Substring(0, entry.Length - 1);
                RefreshDisplay();
            }
        }

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            Submit();
        }
    }

    static bool WasPressed(int digit, Keyboard kb)
    {
        switch (digit)
        {
            case 0: return kb.digit0Key.wasPressedThisFrame || kb.numpad0Key.wasPressedThisFrame;
            case 1: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
            case 2: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
            case 3: return kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame;
            case 4: return kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame;
            case 5: return kb.digit5Key.wasPressedThisFrame || kb.numpad5Key.wasPressedThisFrame;
            case 6: return kb.digit6Key.wasPressedThisFrame || kb.numpad6Key.wasPressedThisFrame;
            case 7: return kb.digit7Key.wasPressedThisFrame || kb.numpad7Key.wasPressedThisFrame;
            case 8: return kb.digit8Key.wasPressedThisFrame || kb.numpad8Key.wasPressedThisFrame;
            case 9: return kb.digit9Key.wasPressedThisFrame || kb.numpad9Key.wasPressedThisFrame;
            default: return false;
        }
    }

    void OpenPad()
    {
        EnsureUi();
        entry = string.Empty;
        open = true;
        overlay.gameObject.SetActive(true);
        RefreshDisplay();
        status.text = "Enter code";
        GameOverlay.Push(this);
        AudioManager.Play(AudioCue.UiClick);
    }

    void ClosePad()
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

    void Append(string digit)
    {
        if (entry.Length >= maxDigits)
        {
            return;
        }

        entry += digit;
        RefreshDisplay();
        AudioManager.Play(AudioCue.UiClick);
    }

    void Submit()
    {
        if (string.Equals(entry, code, System.StringComparison.Ordinal))
        {
            solved = true;
            status.text = "Accepted";
            if (unlocks != null)
            {
                unlocks.Unlock(openAfter: true);
            }

            PuzzleManager manager = PuzzleManager.Instance;
            if (manager != null)
            {
                manager.MarkSolved(puzzleId);
            }

            ClosePad();
            return;
        }

        entry = string.Empty;
        RefreshDisplay();
        status.text = "Wrong code";
        AudioManager.Play(AudioCue.UiClick);
    }

    void RefreshDisplay()
    {
        if (display != null)
        {
            display.text = string.IsNullOrEmpty(entry) ? "—" : entry;
        }
    }

    void EnsureUi()
    {
        if (overlay != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("KeypadCanvas", typeof(RectTransform));
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
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 640f));
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 8f, new RectOffset(28, 28, 24, 24));

        UiKit.CreateText("Title", panel.transform, "Keypad", 32f, UiKit.Accent, TextAlignmentOptions.Center);
        display = UiKit.CreateText("Display", panel.transform, "—", 40f, UiKit.Text, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(display, 52f);
        status = UiKit.CreateText("Status", panel.transform, string.Empty, 18f, UiKit.TextDim, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(status, 28f);

        for (int row = 0; row < 4; row++)
        {
            RectTransform line = UiKit.CreateRect("Row" + row, panel.transform);
            UiKit.SetLayoutHeight(line, 56f);
            HorizontalLayoutGroup layout = line.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            if (row < 3)
            {
                for (int col = 1; col <= 3; col++)
                {
                    int digit = row * 3 + col;
                    AddDigitButton(line, digit);
                }
            }
            else
            {
                UiKit.CreateButton("Clear", line, "C", () =>
                {
                    entry = string.Empty;
                    RefreshDisplay();
                }, 56f);
                AddDigitButton(line, 0);
                UiKit.CreateButton("Enter", line, "OK", Submit, 56f);
            }
        }

        UiKit.CreateButton("Close", panel.transform, "Close", ClosePad, 48f);
        overlay.gameObject.SetActive(false);
    }

    void AddDigitButton(Transform parent, int digit)
    {
        string label = digit.ToString();
        UiKit.CreateButton("D" + label, parent, label, () => Append(label), 56f);
    }
}
