using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Win / lose / restart. Optional countdown (0 = off, HUD shows elapsed time instead).
/// Attach to the GameSystems object (SystemsBootstrap adds one if missing).
/// </summary>
public class GameFlow : MonoBehaviour
{
    public const string DefaultGameplayScene = "Writers Study Greybox";
    public const string DefaultMainMenuScene = "MainMenu";

    [Header("Scenes")]
    [SerializeField] string gameplayScene = DefaultGameplayScene;
    [SerializeField] string mainMenuScene = DefaultMainMenuScene;

    [Header("Timer")]
    [Tooltip("Seconds until lose. 0 disables the countdown (elapsed time still shows).")]
    [SerializeField] float countdownSeconds;
    [SerializeField] float lowTimeSeconds = 30f;

    [Header("End copy")]
    [SerializeField] string winTitle = "You escaped";
    [TextArea] [SerializeField] string winBody = "The study door opens. Cold air, and the rest of the building, wait beyond.";
    [SerializeField] string loseTitle = "Time is up";
    [TextArea] [SerializeField] string loseBody = "The lamps die. Whatever kept you here is not finished with you.";

    static GameFlow instance;

    bool ended;
    float remaining;
    RectTransform endRoot;
    TextMeshProUGUI endTitle;
    TextMeshProUGUI endBody;
    bool built;

    public static GameFlow Instance => instance;
    public bool IsEnded => ended;
    public bool UsesCountdown => countdownSeconds > 0.01f;
    public float RemainingSeconds => remaining;
    public bool IsLowTime => UsesCountdown && !ended && remaining <= lowTimeSeconds;
    public string GameplayScene => string.IsNullOrEmpty(gameplayScene) ? DefaultGameplayScene : gameplayScene;
    public string MainMenuScene => string.IsNullOrEmpty(mainMenuScene) ? DefaultMainMenuScene : mainMenuScene;

    public event Action Won;
    public event Action Lost;

    void Awake()
    {
        instance = this;
        remaining = countdownSeconds;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        if (ended || !UsesCountdown)
        {
            return;
        }

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            Lose();
        }
    }

    public void Win()
    {
        if (ended)
        {
            return;
        }

        ended = true;
        GameOverlay.CloseAll();
        AudioManager.Play(AudioCue.PuzzleSolved);
        CameraImpulse.Shake(0.25f, 0.35f);
        ShowEnd(winTitle, winBody);
        Won?.Invoke();
        Debug.Log("[GameFlow] Win", this);
    }

    public void Lose()
    {
        if (ended)
        {
            return;
        }

        ended = true;
        GameOverlay.CloseAll();
        AudioManager.Play(AudioCue.UiClick);
        ShowEnd(loseTitle, loseBody);
        Lost?.Invoke();
        Debug.Log("[GameFlow] Lose", this);
    }

    public void RestartLevel()
    {
        ended = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameSettings.Save();
        LoadScene(GameplayScene);
    }

    public void ReturnToMenu()
    {
        ended = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameSettings.Save();
        LoadScene(MainMenuScene);
    }

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[GameFlow] Scene name is empty.");
            return;
        }

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.Load(sceneName);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning("[GameFlow] Scene '" + sceneName + "' is not in Build Settings.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    void ShowEnd(string title, string body)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ControlEnabled = false;
        }

        Time.timeScale = 0f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        BuildEndUi();
        endTitle.text = title;
        endBody.text = body;
        endRoot.gameObject.SetActive(true);
    }

    void BuildEndUi()
    {
        if (built)
        {
            return;
        }

        built = true;
        GameObject canvasObject = new GameObject("EndScreenCanvas", typeof(RectTransform));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Image dim = UiKit.CreateImage("Dim", canvasObject.transform, UiKit.Overlay, false, true);
        endRoot = dim.rectTransform;
        UiKit.Stretch(endRoot);

        Image panel = UiKit.CreateImage("Panel", endRoot, UiKit.Panel, true, true);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 420f));
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 12f, new RectOffset(40, 40, 32, 32));

        endTitle = UiKit.CreateText("Title", panel.transform, string.Empty, 44f, UiKit.Accent, TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(endTitle, 64f);
        endBody = UiKit.CreateText("Body", panel.transform, string.Empty, 22f, UiKit.Text, TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(endBody, 120f, 1f);

        UiKit.CreateButton("Restart", panel.transform, "Try Again", RestartLevel);
        if (Application.CanStreamedLevelBeLoaded(MainMenuScene) && SceneManager.GetActiveScene().name != MainMenuScene)
        {
            UiKit.CreateButton("Menu", panel.transform, "Main Menu", ReturnToMenu);
        }
    }
}
