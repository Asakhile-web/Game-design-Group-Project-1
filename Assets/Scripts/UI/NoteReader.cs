using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Full-screen note. Pauses look/move (not timeScale). Close with E or Esc.
/// </summary>
public class NoteReader : MonoBehaviour, IEscapableOverlay
{
    static NoteReader instance;

    RectTransform overlay;
    TextMeshProUGUI title;
    TextMeshProUGUI body;
    Image illustration;
    bool open;
    InputAction closeAction;

    public static NoteReader Instance => instance;
    public bool IsOpen => open;

    void Awake()
    {
        instance = this;
        closeAction = new InputAction("CloseNote", InputActionType.Button);
        closeAction.AddBinding("<Keyboard>/e");
        closeAction.performed += _ =>
        {
            if (open)
            {
                Close();
            }
        };
    }

    void OnEnable()
    {
        closeAction?.Enable();
    }

    void OnDisable()
    {
        closeAction?.Disable();
    }

    void OnDestroy()
    {
        closeAction?.Dispose();
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Open(ClueDefinition clue)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("NoteReader");
            instance = go.AddComponent<NoteReader>();
        }

        instance.Show(clue);
    }

    public void Show(ClueDefinition clue)
    {
        if (clue == null)
        {
            Debug.LogWarning("[NoteReader] Clue is missing.", this);
            return;
        }

        EnsureUi();
        title.text = clue.Title;
        body.text = string.IsNullOrEmpty(clue.Description) ? string.Empty : clue.Description;
        bool hasImage = clue.Image != null;
        illustration.enabled = hasImage;
        illustration.sprite = clue.Image;
        overlay.gameObject.SetActive(true);
        open = true;
        GameOverlay.Push(this);
        AudioManager.Play(AudioCue.Note);
    }

    public void CloseFromEscape()
    {
        Close();
    }

    public void Close()
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

    void EnsureUi()
    {
        if (overlay != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("NoteCanvas", typeof(RectTransform));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        Image dim = UiKit.CreateImage("Dim", canvasObject.transform, UiKit.Overlay, false, true);
        overlay = dim.rectTransform;
        UiKit.Stretch(overlay);

        Image panel = UiKit.CreateImage("Paper", overlay, new Color(0.9f, 0.84f, 0.7f, 0.98f), true, true);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 720f));
        UiKit.AddOutline(panel.rectTransform, new Color(0.35f, 0.22f, 0.1f, 0.8f), 3f);
        UiKit.AddVerticalLayout(panel.gameObject, 10f, new RectOffset(48, 48, 40, 36));

        title = UiKit.CreateText("Title", panel.transform, string.Empty, 36f, new Color(0.22f, 0.14f, 0.08f, 1f), TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(title, 52f);

        illustration = UiKit.CreateImage("Image", panel.transform, Color.white, false);
        UiKit.SetLayoutHeight(illustration, 180f);
        illustration.preserveAspect = true;
        illustration.enabled = false;

        body = UiKit.CreateText("Body", panel.transform, string.Empty, 24f, new Color(0.18f, 0.12f, 0.08f, 1f), TextAlignmentOptions.TopLeft, wrap: true);
        UiKit.SetLayoutHeight(body, 280f, 1f);

        UiKit.CreateText("Hint", panel.transform, "Press E or Esc to put the note away", 18f, new Color(0.3f, 0.2f, 0.12f, 0.8f), TextAlignmentOptions.Center);
        overlay.gameObject.SetActive(false);
    }
}
