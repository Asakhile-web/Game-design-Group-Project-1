using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crosshair + "Press E to …" prompt. Builds a Canvas at runtime if Inspector refs are empty so Play Mode still works.
/// </summary>
public class InteractionHUD : MonoBehaviour
{
    [SerializeField] InteractionRaycaster raycaster;
    [SerializeField] RectTransform crosshair;
    [SerializeField] TextMeshProUGUI promptLabel;
    [SerializeField] Color idleCrosshair = new Color(0.92f, 0.86f, 0.74f, 0.85f);
    [SerializeField] Color hoverCrosshair = new Color(0.95f, 0.72f, 0.28f, 1f);
    [SerializeField] float idleSize = 8f;
    [SerializeField] float hoverSize = 12f;

    Image crosshairImage;
    TextMeshProUGUI toastLabel;
    float toastUntil;
    static InteractionHUD instance;

    public InteractionRaycaster Raycaster
    {
        get => raycaster;
        set => raycaster = value;
    }

    public static void Toast(string message, float seconds = 2f)
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<InteractionHUD>();
        }

        if (instance == null)
        {
            Debug.Log("[Toast] " + message);
            return;
        }

        instance.ShowToast(message, seconds);
    }

    void OnEnable()
    {
        instance = this;
    }

    void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Awake()
    {
        instance = this;
        EnsureUi();
    }

    public void ShowToast(string message, float seconds = 2f)
    {
        EnsureUi();
        if (toastLabel == null)
        {
            Debug.Log("[Toast] " + message);
            return;
        }

        toastLabel.text = message;
        toastUntil = Time.unscaledTime + seconds;
        toastLabel.enabled = true;
    }

    void LateUpdate()
    {
        bool hover = raycaster != null && raycaster.HasTarget;
        if (promptLabel != null)
        {
            string line = hover ? raycaster.CurrentPrompt : string.Empty;
            if (hover && !string.IsNullOrEmpty(raycaster.CurrentUsePrompt) && raycaster.CurrentUsePrompt != line)
            {
                line = string.IsNullOrEmpty(line)
                    ? raycaster.CurrentUsePrompt
                    : line + "\n" + raycaster.CurrentUsePrompt;
            }

            promptLabel.text = line;
            promptLabel.enabled = hover && !string.IsNullOrEmpty(line);
        }

        if (crosshair != null)
        {
            float size = hover ? hoverSize : idleSize;
            crosshair.sizeDelta = Vector2.Lerp(crosshair.sizeDelta, new Vector2(size, size), 18f * Time.deltaTime);
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = Color.Lerp(crosshairImage.color, hover ? hoverCrosshair : idleCrosshair, 18f * Time.deltaTime);
        }

        if (toastLabel != null && Time.unscaledTime >= toastUntil)
        {
            toastLabel.text = string.Empty;
            toastLabel.enabled = false;
        }
    }

    void EnsureUi()
    {
        if (promptLabel != null && crosshair != null && toastLabel != null)
        {
            crosshairImage = crosshair.GetComponent<Image>();
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (crosshair == null)
        {
            GameObject cross = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            cross.transform.SetParent(transform, false);
            crosshair = cross.GetComponent<RectTransform>();
            crosshair.anchorMin = new Vector2(0.5f, 0.5f);
            crosshair.anchorMax = new Vector2(0.5f, 0.5f);
            crosshair.sizeDelta = new Vector2(idleSize, idleSize);
            crosshairImage = cross.GetComponent<Image>();
            crosshairImage.color = idleCrosshair;
            crosshairImage.raycastTarget = false;
        }
        else
        {
            crosshairImage = crosshair.GetComponent<Image>();
        }

        if (promptLabel == null)
        {
            GameObject prompt = new GameObject("Prompt", typeof(RectTransform), typeof(TextMeshProUGUI));
            prompt.transform.SetParent(transform, false);
            RectTransform rect = prompt.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.28f);
            rect.anchorMax = new Vector2(0.5f, 0.28f);
            rect.sizeDelta = new Vector2(800f, 80f);
            promptLabel = prompt.GetComponent<TextMeshProUGUI>();
            promptLabel.alignment = TextAlignmentOptions.Center;
            promptLabel.fontSize = 28f;
            promptLabel.color = idleCrosshair;
            promptLabel.raycastTarget = false;
            promptLabel.text = string.Empty;
            promptLabel.enableWordWrapping = true;
        }

        if (toastLabel == null)
        {
            GameObject toast = new GameObject("Toast", typeof(RectTransform), typeof(TextMeshProUGUI));
            toast.transform.SetParent(transform, false);
            RectTransform toastRect = toast.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 0.18f);
            toastRect.anchorMax = new Vector2(0.5f, 0.18f);
            toastRect.sizeDelta = new Vector2(900f, 40f);
            toastLabel = toast.GetComponent<TextMeshProUGUI>();
            toastLabel.alignment = TextAlignmentOptions.Center;
            toastLabel.fontSize = 24f;
            toastLabel.color = hoverCrosshair;
            toastLabel.raycastTarget = false;
            toastLabel.text = string.Empty;
            toastLabel.enabled = false;
        }
    }
}
