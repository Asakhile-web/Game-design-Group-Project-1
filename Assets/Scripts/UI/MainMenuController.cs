using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Title screen built at runtime (same parchment palette as in-game UI). Put this on an empty in MainMenu.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] string gameplayScene = GameFlow.DefaultGameplayScene;
    [SerializeField] string gameTitle = "Writer's Study";
    [SerializeField] string subtitle = "A study escape";

    enum Page
    {
        Root,
        Settings,
        Credits,
        Controls
    }

    RectTransform pageRoot;
    Page page = Page.Root;
    GameObject rootPage;
    GameObject settingsPage;
    GameObject creditsPage;
    GameObject controlsPage;
    Transform cameraTransform;
    Vector3 cameraRest;

    void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioManager.Ensure();
        SceneFader.Ensure();
        GameSettings.ApplySaved(null);
        EnsureEventSystem();
        EnsureBackdrop();
        BuildUi();
    }

    void Update()
    {
        if (cameraTransform == null)
        {
            return;
        }

        float t = Time.unscaledTime * 0.12f;
        cameraTransform.position = cameraRest + new Vector3(Mathf.Sin(t) * 0.15f, Mathf.Sin(t * 0.7f) * 0.05f, 0f);
        cameraTransform.localEulerAngles = new Vector3(4f + Mathf.Sin(t * 0.5f) * 1.2f, Mathf.Sin(t * 0.35f) * 2f, 0f);
    }

    void Play()
    {
        AudioManager.Play(AudioCue.UiClick);
        string scene = string.IsNullOrEmpty(gameplayScene) ? GameFlow.DefaultGameplayScene : gameplayScene;
        GameFlow.LoadScene(scene);
    }

    void Show(Page next)
    {
        AudioManager.Play(AudioCue.UiClick);
        page = next;
        rootPage.SetActive(page == Page.Root);
        settingsPage.SetActive(page == Page.Settings);
        creditsPage.SetActive(page == Page.Credits);
        controlsPage.SetActive(page == Page.Controls);
    }

    void Quit()
    {
        GameSettings.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void BuildUi()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Image wash = UiKit.CreateImage("Wash", canvasObject.transform, new Color(0.04f, 0.03f, 0.02f, 0.35f), false);
        UiKit.Stretch(wash.rectTransform);

        pageRoot = UiKit.CreateRect("Pages", canvasObject.transform);
        UiKit.Place(pageRoot, new Vector2(0.22f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 780f));

        rootPage = BuildRoot();
        settingsPage = BuildSettings();
        creditsPage = BuildCredits();
        controlsPage = BuildControls();
        Show(Page.Root);
    }

    GameObject BuildRoot()
    {
        Image panel = UiKit.CreateImage("Root", pageRoot, UiKit.Panel, true, true);
        UiKit.Stretch(panel.rectTransform);
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 12f, new RectOffset(48, 48, 40, 40));

        TextMeshProUGUI title = UiKit.CreateText("Title", panel.transform, gameTitle, 52f, UiKit.Accent, TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(title, 80f);
        UiKit.CreateText("Sub", panel.transform, subtitle, 22f, UiKit.TextDim, TextAlignmentOptions.Center);
        RectTransform spacer = UiKit.CreateRect("Sp", panel.transform);
        UiKit.SetLayoutHeight(spacer, 12f);

        UiKit.CreateButton("Play", panel.transform, "Play", Play);
        UiKit.CreateButton("Settings", panel.transform, "Settings", () => Show(Page.Settings));
        UiKit.CreateButton("Controls", panel.transform, "Controls", () => Show(Page.Controls));
        UiKit.CreateButton("Credits", panel.transform, "Credits", () => Show(Page.Credits));
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            UiKit.CreateButton("Quit", panel.transform, "Quit", Quit);
        }

        return panel.gameObject;
    }

    GameObject BuildSettings()
    {
        Image panel = UiKit.CreateImage("Settings", pageRoot, UiKit.Panel, true, true);
        UiKit.Stretch(panel.rectTransform);
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 10f, new RectOffset(40, 40, 32, 32));
        UiKit.CreateText("Title", panel.transform, "Settings", 40f, UiKit.Accent, TextAlignmentOptions.Center);

        AddSlider(panel.transform, "Master Volume", GameSettings.MasterVolume, 0f, 1f, v => GameSettings.MasterVolume = v);
        AddSlider(panel.transform, "Music", GameSettings.MusicVolume, 0f, 1f, v => GameSettings.MusicVolume = v);
        AddSlider(panel.transform, "SFX", GameSettings.SfxVolume, 0f, 1f, v => GameSettings.SfxVolume = v);
        AddSlider(panel.transform, "Mouse Sensitivity", GameSettings.GetMouseSensitivity(0.12f), GameSettings.MinSensitivity, GameSettings.MaxSensitivity,
            v => GameSettings.SetMouseSensitivity(v, null));

        RectTransform bobRow = UiKit.CreateSettingRow(panel.transform, "Head Bob", out _);
        UiKit.CreateToggle(bobRow, GameSettings.HeadBobEnabled, v => GameSettings.SetHeadBob(v, null));

        if (GameSettings.FullscreenSupported)
        {
            RectTransform fs = UiKit.CreateSettingRow(panel.transform, "Fullscreen", out _);
            UiKit.CreateToggle(fs, GameSettings.Fullscreen, v => GameSettings.Fullscreen = v);
        }

        RectTransform spacer = UiKit.CreateRect("Sp", panel.transform);
        UiKit.SetLayoutHeight(spacer, 0f, 1f);
        UiKit.CreateButton("Back", panel.transform, "Back", () => Show(Page.Root));
        return panel.gameObject;
    }

    GameObject BuildCredits()
    {
        Image panel = UiKit.CreateImage("Credits", pageRoot, UiKit.Panel, true, true);
        UiKit.Stretch(panel.rectTransform);
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 10f, new RectOffset(40, 40, 32, 32));
        UiKit.CreateText("Title", panel.transform, "Credits", 40f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.CreateText("Body", panel.transform,
            "A university group project.\n\nScripting and systems — programming team\nModels, textures, animation — art team\nAudio — sound team\n\nBuilt in Unity 6 / URP with no Asset Store packs.",
            22f, UiKit.Text, TextAlignmentOptions.Center, wrap: true);
        RectTransform spacer = UiKit.CreateRect("Sp", panel.transform);
        UiKit.SetLayoutHeight(spacer, 0f, 1f);
        UiKit.CreateButton("Back", panel.transform, "Back", () => Show(Page.Root));
        return panel.gameObject;
    }

    GameObject BuildControls()
    {
        Image panel = UiKit.CreateImage("Controls", pageRoot, UiKit.Panel, true, true);
        UiKit.Stretch(panel.rectTransform);
        UiKit.AddOutline(panel.rectTransform, UiKit.AccentDim);
        UiKit.AddVerticalLayout(panel.gameObject, 8f, new RectOffset(40, 40, 32, 32));
        UiKit.CreateText("Title", panel.transform, "Controls", 40f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.CreateText("Body", panel.transform,
            "WASD — Move\nMouse — Look\nLeft Shift — Sprint\nLeft Ctrl — Crouch\nE — Interact / close note\nF or Left Click — Use selected item\n1–5 — Hotbar\nTab / I — Inventory\nJ — Journal\nH — Hint (limited)\nEsc — Pause\nHold Alt — Free cursor",
            22f, UiKit.Text, TextAlignmentOptions.TopLeft, wrap: true);
        RectTransform spacer = UiKit.CreateRect("Sp", panel.transform);
        UiKit.SetLayoutHeight(spacer, 0f, 1f);
        UiKit.CreateButton("Back", panel.transform, "Back", () => Show(Page.Root));
        return panel.gameObject;
    }

    static void AddSlider(Transform parent, string label, float value, float min, float max, UnityEngine.Events.UnityAction<float> onChanged)
    {
        RectTransform row = UiKit.CreateSettingRow(parent, label, out TextMeshProUGUI valueLabel);
        valueLabel.text = min >= 0f && max <= 1.01f ? Mathf.RoundToInt(value * 100f) + "%" : value.ToString("0.00");
        UiKit.CreateSlider(row, min, max, value, v =>
        {
            onChanged(v);
            valueLabel.text = min >= 0f && max <= 1.01f ? Mathf.RoundToInt(v * 100f) + "%" : v.ToString("0.00");
        });
    }

    void EnsureBackdrop()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObject = new GameObject("MenuCamera");
            cam = camObject.AddComponent<Camera>();
            camObject.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.04f);
        }

        cameraTransform = cam.transform;
        cameraRest = new Vector3(0f, 1.4f, -3.2f);
        cameraTransform.position = cameraRest;

        if (GameObject.Find("MenuBackdrop") != null)
        {
            return;
        }

        GameObject root = new GameObject("MenuBackdrop");
        CreatePrimitive(root.transform, PrimitiveType.Cube, new Vector3(0f, -0.05f, 2f), new Vector3(12f, 0.1f, 12f), new Color(0.22f, 0.16f, 0.1f));
        CreatePrimitive(root.transform, PrimitiveType.Cube, new Vector3(0f, 1.6f, 5.2f), new Vector3(12f, 4f, 0.2f), new Color(0.28f, 0.18f, 0.12f));
        CreatePrimitive(root.transform, PrimitiveType.Cube, new Vector3(-1.4f, 0.7f, 2.4f), new Vector3(1.6f, 0.7f, 0.8f), new Color(0.32f, 0.2f, 0.1f));
        CreatePrimitive(root.transform, PrimitiveType.Cube, new Vector3(1.2f, 1.1f, 3.4f), new Vector3(0.9f, 2.2f, 0.35f), new Color(0.2f, 0.14f, 0.1f));

        GameObject lamp = new GameObject("Lamp");
        lamp.transform.SetParent(root.transform);
        lamp.transform.position = new Vector3(-0.4f, 2.4f, 2.1f);
        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.82f, 0.55f);
        light.intensity = 2.2f;
        light.range = 10f;
        lamp.AddComponent<LightFlicker>();
        DustMotes.Spawn(lamp.transform, Vector3.zero);
    }

    static void CreatePrimitive(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }
    }

    static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject go = new GameObject("EventSystem", typeof(EventSystem));
        go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
}
