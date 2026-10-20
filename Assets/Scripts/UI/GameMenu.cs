using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Central pause state + in-scene overlay menus. Every overlay (pause, inventory, journal, settings, confirm)
/// is pushed onto one stack: the game is paused while the stack is non-empty, Escape/Back pops one level,
/// and only popping the last panel resumes gameplay. Pause uses Time.timeScale = 0; GameFlow also
/// freezes time on the win/lose screen.
/// </summary>
public class GameMenu : MonoBehaviour
{
    public enum Screen
    {
        Pause,
        Inventory,
        Journal,
        Settings,
        Confirm
    }

    [SerializeField] string mainMenuScene = "MainMenu";

    static GameMenu instance;

    readonly List<Screen> stack = new List<Screen>();
    readonly Dictionary<Screen, MenuPanel> panels = new Dictionary<Screen, MenuPanel>();

    PlayerController player;
    RectTransform overlay;
    InputAction pauseAction;
    InputAction inventoryAction;
    InputAction journalAction;
    InputAction backAction;
    InputAction freeCursorAction;

    bool paused;
    bool cursorFree;
    bool controlSuspended;
    bool savedControl = true;
    float savedTimeScale = 1f;
    bool built;

    public static GameMenu Instance => instance;
    public bool IsPaused => paused;
    public bool IsCursorFree => cursorFree;
    public int Depth => stack.Count;
    public PlayerController Player => player;
    public InventorySystem Inventory => player != null ? player.Inventory : null;
    public ClueJournal Journal => player != null ? player.GetComponent<ClueJournal>() : null;
    public ObjectiveTracker Objectives => player != null ? player.GetComponent<ObjectiveTracker>() : null;
    public bool CanReturnToMainMenu => !string.IsNullOrEmpty(mainMenuScene) && Application.CanStreamedLevelBeLoaded(mainMenuScene)
                                       && SceneManager.GetActiveScene().name != mainMenuScene;
    public bool CanQuit => Application.platform != RuntimePlatform.WebGLPlayer;

    public event Action<bool> PausedChanged;
    public event Action<bool> CursorFreeChanged;

    public void Bind(PlayerController owner)
    {
        player = owner;
    }

    void Awake()
    {
        instance = this;
        CreateActions();
    }

    void OnEnable()
    {
        pauseAction.Enable();
        inventoryAction.Enable();
        journalAction.Enable();
        backAction.Enable();
        freeCursorAction.Enable();
    }

    void OnDisable()
    {
        pauseAction.Disable();
        inventoryAction.Disable();
        journalAction.Disable();
        backAction.Disable();
        freeCursorAction.Disable();
    }

    void OnDestroy()
    {
        if (paused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        pauseAction?.Dispose();
        inventoryAction?.Dispose();
        journalAction?.Dispose();
        backAction?.Dispose();
        freeCursorAction?.Dispose();
        GameSettings.Save();

        if (instance == this)
        {
            instance = null;
        }
    }

    void Start()
    {
        EnsureEventSystem();
        Build();
        GameSettings.ApplySaved(player);
    }

    void Update()
    {
        bool wantFree = !paused && freeCursorAction.IsPressed();
        if (wantFree != cursorFree)
        {
            cursorFree = wantFree;
            RefreshControlState();
            CursorFreeChanged?.Invoke(cursorFree);
        }
    }

    // ---------- Public navigation API (used by HUD buttons and panels) ----------

    public void OpenPauseMenu() => Open(Screen.Pause);
    public void OpenInventory() => Open(Screen.Inventory);
    public void OpenJournal() => Open(Screen.Journal);
    public void OpenSettings() => Open(Screen.Settings);

    public void TogglePause()
    {
        if (GameFlow.Instance != null && GameFlow.Instance.IsEnded)
        {
            return;
        }

        if (stack.Count == 0 && GameOverlay.TryHandleEscape())
        {
            return;
        }

        if (stack.Count == 0)
        {
            Open(Screen.Pause);
        }
        else
        {
            Back();
        }
    }

    /// <summary>Close the top panel. Resumes gameplay only when no panels remain.</summary>
    public void Back()
    {
        if (stack.Count == 0)
        {
            return;
        }

        stack.RemoveAt(stack.Count - 1);
        if (stack.Count == 0)
        {
            HideAll();
            SetPaused(false);
        }
        else
        {
            ShowTop();
        }
    }

    /// <summary>Close every panel and return to gameplay exactly where the player left off.</summary>
    public void ResumeGame()
    {
        stack.Clear();
        HideAll();
        SetPaused(false);
    }

    public void Confirm(string title, string body, string confirmLabel, Action onConfirm)
    {
        if (panels.TryGetValue(Screen.Confirm, out MenuPanel panel) && panel is ConfirmPanel confirm)
        {
            confirm.Setup(title, body, confirmLabel, onConfirm);
            Open(Screen.Confirm);
        }
    }

    public void RequestMainMenu()
    {
        if (!CanReturnToMainMenu)
        {
            return;
        }

        Confirm("Return to Main Menu?",
            "Progress in this level is not saved. Your clues, inventory and objectives will be lost.",
            "Leave Level",
            () =>
            {
                stack.Clear();
                HideAll();
                SetPaused(false);
                Time.timeScale = 1f;
                AudioListener.pause = false;
                GameSettings.Save();
                GameFlow.LoadScene(mainMenuScene);
            });
    }

    public void RequestQuit()
    {
        if (!CanQuit)
        {
            return;
        }

        Confirm("Quit Game?",
            "Progress in this level is not saved and will be lost.",
            "Quit",
            () =>
            {
                GameSettings.Save();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
    }

    // ---------- Stack handling ----------

    void Open(Screen screen)
    {
        if (!built)
        {
            Build();
        }

        int existing = stack.IndexOf(screen);
        if (existing >= 0)
        {
            stack.RemoveRange(existing + 1, stack.Count - existing - 1);
        }
        else
        {
            stack.Add(screen);
        }

        SetPaused(true);
        ShowTop();
    }

    Screen? Top => stack.Count > 0 ? stack[stack.Count - 1] : (Screen?)null;

    void ShowTop()
    {
        Screen top = stack[stack.Count - 1];
        overlay.gameObject.SetActive(true);
        foreach (KeyValuePair<Screen, MenuPanel> pair in panels)
        {
            pair.Value.gameObject.SetActive(pair.Key == top);
        }

        MenuPanel panel = panels[top];
        panel.OnShow();

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            Selectable first = Gamepad.current != null ? panel.FirstSelectable : null;
            eventSystem.SetSelectedGameObject(first != null ? first.gameObject : null);
        }
    }

    void HideAll()
    {
        if (overlay != null)
        {
            overlay.gameObject.SetActive(false);
        }

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    void SetPaused(bool value)
    {
        if (paused == value)
        {
            return;
        }

        paused = value;
        if (paused)
        {
            savedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            cursorFree = false;
        }
        else
        {
            Time.timeScale = savedTimeScale;
            AudioListener.pause = false;
            GameSettings.Save();
        }

        RefreshControlState();
        PausedChanged?.Invoke(paused);
    }

    /// <summary>Player input is suspended while paused or while the cursor is freed to click HUD buttons.</summary>
    void RefreshControlState()
    {
        bool suspend = paused || cursorFree;
        if (player != null)
        {
            if (suspend && !controlSuspended)
            {
                savedControl = player.ControlEnabled;
                player.ControlEnabled = false;
                controlSuspended = true;
            }
            else if (!suspend && controlSuspended)
            {
                player.ControlEnabled = savedControl;
                controlSuspended = false;
            }
        }

        if (suspend)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (savedControl)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // ---------- Input ----------

    void CreateActions()
    {
        pauseAction = new InputAction("MenuPause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseAction.performed += _ => TogglePause();

        inventoryAction = new InputAction("MenuInventory", InputActionType.Button);
        inventoryAction.AddBinding("<Keyboard>/tab");
        inventoryAction.AddBinding("<Keyboard>/i");
        inventoryAction.AddBinding("<Gamepad>/select");
        inventoryAction.performed += _ => ToggleShortcut(Screen.Inventory);

        journalAction = new InputAction("MenuJournal", InputActionType.Button);
        journalAction.AddBinding("<Keyboard>/j");
        journalAction.performed += _ => ToggleShortcut(Screen.Journal);

        backAction = new InputAction("MenuBack", InputActionType.Button);
        backAction.AddBinding("<Gamepad>/buttonEast");
        backAction.performed += _ =>
        {
            if (stack.Count > 0)
            {
                Back();
            }
        };

        freeCursorAction = new InputAction("FreeCursor", InputActionType.Button);
        freeCursorAction.AddBinding("<Keyboard>/leftAlt");
        freeCursorAction.AddBinding("<Keyboard>/rightAlt");
    }

    /// <summary>Tab / J: open from gameplay or the pause menu, close if already on top. Ignored in settings/confirm.</summary>
    void ToggleShortcut(Screen screen)
    {
        Screen? top = Top;
        if (top == screen)
        {
            Back();
            return;
        }

        if (top == null || top == Screen.Pause || top == Screen.Inventory || top == Screen.Journal)
        {
            if (top == Screen.Inventory || top == Screen.Journal)
            {
                stack.RemoveAt(stack.Count - 1);
            }

            Open(screen);
        }
    }

    // ---------- Construction ----------

    void Build()
    {
        if (built)
        {
            return;
        }

        built = true;

        GameObject canvasObject = new GameObject("GameMenuCanvas", typeof(RectTransform));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Image dim = UiKit.CreateImage("Overlay", canvasObject.transform, UiKit.Overlay, false, true);
        overlay = dim.rectTransform;
        UiKit.Stretch(overlay);

        AddPanel<PausePanel>(Screen.Pause, new Vector2(520f, 640f));
        AddPanel<InventoryPanel>(Screen.Inventory, new Vector2(1080f, 640f));
        AddPanel<JournalPanel>(Screen.Journal, new Vector2(1180f, 700f));
        AddPanel<SettingsPanel>(Screen.Settings, new Vector2(820f, 720f));
        AddPanel<ConfirmPanel>(Screen.Confirm, new Vector2(680f, 320f));

        HideAll();
    }

    void AddPanel<T>(Screen screen, Vector2 size) where T : MenuPanel
    {
        RectTransform rect = UiKit.CreateRect(screen + "Panel", overlay);
        T panel = rect.gameObject.AddComponent<T>();
        panel.Initialize(this, size);
        rect.gameObject.SetActive(false);
        panels[screen] = panel;
    }

    static void EnsureEventSystem()
    {
        if (EventSystem.current != null || FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject go = new GameObject("EventSystem", typeof(EventSystem));
        InputSystemUIInputModule module = go.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
    }
}

/// <summary>Resume / Inventory / Journal / Settings / Main Menu / Quit.</summary>
public class PausePanel : MenuPanel
{
    Button resume;
    Button mainMenu;
    Button quit;
    TextMeshProUGUI objective;

    public override Selectable FirstSelectable => resume;

    protected override void Build()
    {
        UiKit.AddVerticalLayout(gameObject, 12f, new RectOffset(48, 48, 40, 40));

        TextMeshProUGUI title = UiKit.CreateText("Title", Root, "Paused", 52f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(title, 70f);

        objective = UiKit.CreateText("Objective", Root, string.Empty, 20f, UiKit.TextDim, TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(objective, 56f);

        resume = UiKit.CreateButton("Resume", Root, "Resume", () => Menu.ResumeGame());
        UiKit.CreateButton("Inventory", Root, "Inventory", () => Menu.OpenInventory());
        UiKit.CreateButton("Journal", Root, "Clues / Journal", () => Menu.OpenJournal());
        UiKit.CreateButton("Settings", Root, "Settings", () => Menu.OpenSettings());
        mainMenu = UiKit.CreateButton("MainMenu", Root, "Main Menu", () => Menu.RequestMainMenu());
        quit = UiKit.CreateButton("Quit", Root, "Quit Game", () => Menu.RequestQuit());
    }

    public override void OnShow()
    {
        mainMenu.gameObject.SetActive(Menu.CanReturnToMainMenu);
        quit.gameObject.SetActive(Menu.CanQuit);

        ObjectiveTracker tracker = Menu.Objectives;
        objective.text = tracker != null && tracker.HasObjective ? "Objective: " + tracker.Current : string.Empty;
    }
}

/// <summary>Generic yes/no dialog; Cancel (or Escape) returns to the previous panel.</summary>
public class ConfirmPanel : MenuPanel
{
    TextMeshProUGUI title;
    TextMeshProUGUI body;
    Button confirm;
    Button cancel;
    Action onConfirm;

    public override Selectable FirstSelectable => cancel;

    public void Setup(string heading, string message, string confirmLabel, Action action)
    {
        title.text = heading;
        body.text = message;
        UiKit.GetLabel(confirm).text = confirmLabel;
        onConfirm = action;
    }

    protected override void Build()
    {
        UiKit.AddVerticalLayout(gameObject, 14f, new RectOffset(40, 40, 32, 32));

        title = UiKit.CreateText("Title", Root, string.Empty, 34f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(title, 48f);

        body = UiKit.CreateText("Body", Root, string.Empty, 22f, UiKit.Text, TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(body, 90f, 1f);

        RectTransform row = UiKit.CreateRect("Buttons", Root);
        UiKit.SetLayoutHeight(row, 56f);
        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        cancel = UiKit.CreateButton("Cancel", row, "Cancel", () => Menu.Back());
        confirm = UiKit.CreateButton("Confirm", row, "Confirm", () =>
        {
            Action action = onConfirm;
            onConfirm = null;
            action?.Invoke();
        });
    }
}
