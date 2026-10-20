using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Persistent gameplay HUD layered on the InteractionHUD canvas:
/// top-left objective + timer, top-right pause and journal buttons, bottom-right inventory shortcut,
/// bottom-left control hints, and centre notifications for new clues / objectives.
/// The crosshair, interaction prompt and hotbar stay in InteractionHUD / HotbarHUD.
/// </summary>
public class GameHUD : MonoBehaviour
{
    const float Margin = 28f;
    const float NotificationSeconds = 3.2f;
    const float FadeSeconds = 0.35f;
    const float HintSeconds = 14f;

    struct Notification
    {
        public string Title;
        public string Subtitle;
    }

    PlayerController player;
    GameMenu menu;
    InventorySystem inventory;
    ClueJournal journal;
    ObjectiveTracker objectives;

    RectTransform hudRoot;
    CanvasGroup hudGroup;
    GameObject objectivePanel;
    TextMeshProUGUI objectiveCaption;
    TextMeshProUGUI objectiveText;
    TextMeshProUGUI timerText;
    TextMeshProUGUI inventoryItemText;
    TextMeshProUGUI journalCountText;
    CanvasGroup hintGroup;
    TextMeshProUGUI hintText;
    CanvasGroup notificationGroup;
    TextMeshProUGUI notificationTitle;
    TextMeshProUGUI notificationSubtitle;

    readonly Queue<Notification> pending = new Queue<Notification>();
    bool showingNotification;
    float notificationTime;
    float hintTime;
    bool built;

    public void Bind(PlayerController owner, GameMenu gameMenu)
    {
        Unsubscribe();
        player = owner;
        menu = gameMenu;
        inventory = owner != null ? owner.Inventory : null;
        journal = owner != null ? owner.GetComponent<ClueJournal>() : null;
        objectives = owner != null ? owner.GetComponent<ObjectiveTracker>() : null;
        if (isActiveAndEnabled)
        {
            Subscribe();
        }
    }

    public void ShowNotification(string title, string subtitle = null)
    {
        pending.Enqueue(new Notification { Title = title, Subtitle = subtitle });
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Start()
    {
        Build();
        RefreshAll();
    }

    void Subscribe()
    {
        if (inventory != null)
        {
            inventory.Changed += RefreshInventory;
        }

        if (journal != null)
        {
            journal.ClueAdded += OnClueAdded;
        }

        if (objectives != null)
        {
            objectives.ObjectiveChanged += OnObjectiveChanged;
            objectives.ObjectiveCompleted += OnObjectiveCompleted;
        }

        if (menu != null)
        {
            menu.PausedChanged += OnPausedChanged;
            menu.CursorFreeChanged += OnCursorFreeChanged;
        }

        GameSettings.Changed += RefreshObjective;
    }

    void Unsubscribe()
    {
        if (inventory != null)
        {
            inventory.Changed -= RefreshInventory;
        }

        if (journal != null)
        {
            journal.ClueAdded -= OnClueAdded;
        }

        if (objectives != null)
        {
            objectives.ObjectiveChanged -= OnObjectiveChanged;
            objectives.ObjectiveCompleted -= OnObjectiveCompleted;
        }

        if (menu != null)
        {
            menu.PausedChanged -= OnPausedChanged;
            menu.CursorFreeChanged -= OnCursorFreeChanged;
        }

        GameSettings.Changed -= RefreshObjective;
    }

    void OnClueAdded(ClueDefinition clue)
    {
        ShowNotification("New clue: " + clue.Title, "Recorded in your journal - press J to review");
        RefreshJournal();
    }

    void OnObjectiveChanged(string objective)
    {
        RefreshObjective();
        if (!string.IsNullOrEmpty(objective))
        {
            ShowNotification("New objective", objective);
        }
    }

    void OnObjectiveCompleted(string objective)
    {
        ShowNotification("Objective complete", objective);
    }

    void OnPausedChanged(bool paused)
    {
        if (hudGroup != null)
        {
            hudGroup.interactable = !paused;
            hudGroup.blocksRaycasts = !paused;
        }
    }

    void OnCursorFreeChanged(bool free)
    {
        if (hintText == null)
        {
            return;
        }

        if (free)
        {
            hintText.text = "Cursor free - click the HUD buttons. Release Alt to look around.";
            hintGroup.alpha = 1f;
        }
        else
        {
            hintText.text = ControlsHint;
            hintTime = HintSeconds - 2f;
        }
    }

    static string ControlsHint =>
        "<b>WASD</b> move   <b>Shift</b> sprint   <b>Ctrl</b> crouch   <b>E</b> interact   <b>F</b> use item\n" +
        "<b>1-5</b> select   <b>Tab</b> inventory   <b>J</b> journal   <b>H</b> hint   <b>Esc</b> pause   <b>Hold Alt</b> cursor";

    void LateUpdate()
    {
        if (!built)
        {
            return;
        }

        if (timerText != null && timerText.gameObject.activeSelf)
        {
            GameFlow flow = GameFlow.Instance;
            if (flow != null && flow.UsesCountdown)
            {
                timerText.text = UiKit.FormatTime(flow.RemainingSeconds);
                timerText.color = flow.IsLowTime ? new Color(0.95f, 0.35f, 0.28f) : UiKit.TextDim;
            }
            else if (objectives != null)
            {
                timerText.text = UiKit.FormatTime(objectives.ElapsedSeconds);
                timerText.color = UiKit.TextDim;
            }
        }

        bool paused = menu != null && menu.IsPaused;
        if (!paused && (menu == null || !menu.IsCursorFree))
        {
            hintTime += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01((hintTime - HintSeconds) / 1.5f);
            hintGroup.alpha = alpha;
        }

        TickNotifications(paused);
    }

    void TickNotifications(bool paused)
    {
        if (paused)
        {
            return;
        }

        if (!showingNotification)
        {
            if (pending.Count == 0)
            {
                notificationGroup.alpha = 0f;
                return;
            }

            Notification next = pending.Dequeue();
            notificationTitle.text = next.Title;
            notificationSubtitle.text = next.Subtitle ?? string.Empty;
            notificationSubtitle.gameObject.SetActive(!string.IsNullOrEmpty(next.Subtitle));
            notificationTime = 0f;
            showingNotification = true;
        }

        notificationTime += Time.unscaledDeltaTime;
        float fadeIn = Mathf.Clamp01(notificationTime / FadeSeconds);
        float fadeOut = Mathf.Clamp01((NotificationSeconds - notificationTime) / FadeSeconds);
        notificationGroup.alpha = Mathf.Min(fadeIn, fadeOut);
        if (notificationTime >= NotificationSeconds)
        {
            showingNotification = false;
            notificationGroup.alpha = 0f;
        }
    }

    void RefreshAll()
    {
        RefreshObjective();
        RefreshInventory();
        RefreshJournal();
    }

    void RefreshObjective()
    {
        if (!built)
        {
            return;
        }

        bool hasObjective = objectives != null && objectives.HasObjective;
        bool showTimer = objectives != null && GameSettings.ShowTimer;
        objectiveCaption.gameObject.SetActive(hasObjective);
        objectiveText.gameObject.SetActive(hasObjective);
        objectiveText.text = hasObjective ? objectives.Current : string.Empty;
        timerText.gameObject.SetActive(showTimer);
        objectivePanel.SetActive(hasObjective || showTimer);
    }

    void RefreshInventory()
    {
        if (!built)
        {
            return;
        }

        ItemDefinition selected = inventory != null ? inventory.SelectedItem : null;
        inventoryItemText.text = selected != null ? selected.DisplayName : "No item selected";
        inventoryItemText.color = selected != null ? UiKit.Accent : UiKit.TextDim;
    }

    void RefreshJournal()
    {
        if (!built)
        {
            return;
        }

        int count = journal != null ? journal.Count : 0;
        journalCountText.text = count > 0 ? count.ToString() : string.Empty;
        journalCountText.transform.parent.gameObject.SetActive(count > 0);
    }

    // ---------- Construction ----------

    void Build()
    {
        if (built)
        {
            return;
        }

        built = true;
        hudRoot = UiKit.CreateRect("GameHUD", transform);
        UiKit.Stretch(hudRoot);
        hudRoot.SetAsFirstSibling();
        hudGroup = hudRoot.gameObject.AddComponent<CanvasGroup>();

        BuildObjectivePanel();
        BuildTopRightButtons();
        BuildInventoryButton();
        BuildHint();
        BuildNotification();
    }

    void BuildObjectivePanel()
    {
        Image panel = UiKit.CreateImage("Objective", hudRoot, UiKit.PanelSoft, true);
        UiKit.Place(panel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Margin, -Margin), new Vector2(460f, 0f));
        objectivePanel = panel.gameObject;
        UiKit.AddVerticalLayout(panel.gameObject, 2f, new RectOffset(20, 20, 14, 14));
        ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        objectiveCaption = UiKit.CreateText("Caption", panel.transform, "OBJECTIVE", 16f, UiKit.Accent);
        objectiveCaption.characterSpacing = 6f;
        UiKit.SetLayoutHeight(objectiveCaption, 22f);

        objectiveText = UiKit.CreateText("Text", panel.transform, string.Empty, 22f, UiKit.Text, TextAlignmentOptions.TopLeft, wrap: true);

        timerText = UiKit.CreateText("Timer", panel.transform, "00:00", 20f, UiKit.TextDim);
        UiKit.SetLayoutHeight(timerText, 28f);
    }

    void BuildTopRightButtons()
    {
        Button pause = CreateIconButton("PauseButton", new Vector2(-Margin, -Margin), () => menu?.OpenPauseMenu());
        Image barLeft = UiKit.CreateImage("BarLeft", pause.transform, UiKit.Text, false);
        UiKit.Place(barLeft.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-7f, 0f), new Vector2(8f, 26f));
        Image barRight = UiKit.CreateImage("BarRight", pause.transform, UiKit.Text, false);
        UiKit.Place(barRight.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(7f, 0f), new Vector2(8f, 26f));
        AddKeyCaption(pause.transform, "Esc");

        Button journalButton = CreateIconButton("JournalButton", new Vector2(-Margin - 80f, -Margin), () => menu?.OpenJournal());
        Image book = UiKit.CreateImage("Book", journalButton.transform, UiKit.Text, true);
        UiKit.Place(book.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26f, 32f));
        Image spine = UiKit.CreateImage("Spine", book.transform, UiKit.Accent, false);
        UiKit.Place(spine.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(4f, 28f));
        AddKeyCaption(journalButton.transform, "J");

        Image badge = UiKit.CreateImage("Badge", journalButton.transform, UiKit.Accent, true);
        UiKit.Place(badge.rectTransform, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-6f, -6f), new Vector2(26f, 26f));
        journalCountText = UiKit.CreateText("Count", badge.transform, string.Empty, 16f, UiKit.Panel, TextAlignmentOptions.Center);
        UiKit.Stretch(journalCountText.rectTransform);
    }

    Button CreateIconButton(string name, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        Image background = UiKit.CreateImage(name, hudRoot, Color.white, true, true);
        UiKit.Place(background.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), position, new Vector2(64f, 64f));
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = UiKit.ButtonColors;
        colors.normalColor = UiKit.PanelSoft;
        button.colors = colors;
        button.onClick.AddListener(onClick);
        return button;
    }

    static void AddKeyCaption(Transform parent, string key)
    {
        TextMeshProUGUI caption = UiKit.CreateText("Key", parent, key, 15f, UiKit.TextDim, TextAlignmentOptions.Center);
        UiKit.Place(caption.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(64f, 20f));
    }

    void BuildInventoryButton()
    {
        Image background = UiKit.CreateImage("InventoryButton", hudRoot, Color.white, true, true);
        UiKit.Place(background.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-Margin, Margin), new Vector2(280f, 72f));
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = UiKit.ButtonColors;
        colors.normalColor = UiKit.PanelSoft;
        button.colors = colors;
        button.onClick.AddListener(() => menu?.OpenInventory());

        Image bag = UiKit.CreateImage("Bag", background.transform, UiKit.Text, true);
        UiKit.Place(bag.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, -3f), new Vector2(34f, 30f));
        Image strap = UiKit.CreateImage("Strap", bag.transform, UiKit.Accent, false);
        UiKit.Place(strap.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, -2f), new Vector2(16f, 8f));

        TextMeshProUGUI title = UiKit.CreateText("Title", background.transform, "Inventory  <color=#FFFFFF88><size=16>Tab</size></color>", 20f, UiKit.Text);
        UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -8f), new Vector2(200f, 28f));

        inventoryItemText = UiKit.CreateText("Selected", background.transform, string.Empty, 18f, UiKit.TextDim);
        UiKit.Place(inventoryItemText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(66f, 10f), new Vector2(200f, 26f));
    }

    void BuildHint()
    {
        Image panel = UiKit.CreateImage("ControlsHint", hudRoot, UiKit.PanelSoft, true);
        UiKit.Place(panel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(Margin, Margin), new Vector2(430f, 72f));
        hintGroup = panel.gameObject.AddComponent<CanvasGroup>();
        hintGroup.blocksRaycasts = false;
        hintText = UiKit.CreateText("Text", panel.transform, ControlsHint, 15f, UiKit.TextDim, TextAlignmentOptions.MidlineLeft, wrap: true);
        UiKit.Stretch(hintText.rectTransform, 14f, 6f, 14f, 6f);
    }

    void BuildNotification()
    {
        Image panel = UiKit.CreateImage("Notification", hudRoot, UiKit.Panel, true);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 0f));
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 4f, new RectOffset(28, 28, 16, 16));
        ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        notificationGroup = panel.gameObject.AddComponent<CanvasGroup>();
        notificationGroup.alpha = 0f;
        notificationGroup.blocksRaycasts = false;
        notificationGroup.interactable = false;

        notificationTitle = UiKit.CreateText("Title", panel.transform, string.Empty, 28f, UiKit.Accent, TextAlignmentOptions.Center, wrap: true);
        notificationSubtitle = UiKit.CreateText("Subtitle", panel.transform, string.Empty, 20f, UiKit.Text, TextAlignmentOptions.Center, wrap: true);
    }
}
