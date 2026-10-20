using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Shared palette and runtime builders for the HUD and pause menus, so every panel matches the
/// warm parchment-on-dark style of InteractionHUD / HotbarHUD without needing prefabs.
/// </summary>
public static class UiKit
{
    public static readonly Color Panel = new Color(0.07f, 0.055f, 0.045f, 0.94f);
    public static readonly Color PanelSoft = new Color(0.07f, 0.055f, 0.045f, 0.62f);
    public static readonly Color Inset = new Color(0.14f, 0.11f, 0.08f, 0.85f);
    public static readonly Color Text = new Color(0.92f, 0.86f, 0.74f, 1f);
    public static readonly Color TextDim = new Color(0.92f, 0.86f, 0.74f, 0.6f);
    public static readonly Color Accent = new Color(0.95f, 0.72f, 0.28f, 1f);
    public static readonly Color AccentDim = new Color(0.95f, 0.72f, 0.28f, 0.35f);
    public static readonly Color Overlay = new Color(0.02f, 0.015f, 0.01f, 0.72f);

    static readonly Color ButtonNormal = new Color(0.16f, 0.12f, 0.09f, 0.95f);
    static readonly Color ButtonHover = new Color(0.34f, 0.23f, 0.12f, 1f);
    static readonly Color ButtonPressed = new Color(0.48f, 0.32f, 0.12f, 1f);
    static readonly Color ButtonDisabled = new Color(0.12f, 0.1f, 0.08f, 0.45f);

    const int SpriteSize = 64;
    const int SpriteRadius = 14;
    static Sprite rounded;

    public static Sprite Rounded
    {
        get
        {
            if (rounded == null)
            {
                rounded = BuildRoundedSprite(SpriteSize, SpriteRadius);
            }

            return rounded;
        }
    }

    public static ColorBlock ButtonColors
    {
        get
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = ButtonNormal;
            colors.highlightedColor = ButtonHover;
            colors.selectedColor = ButtonHover;
            colors.pressedColor = ButtonPressed;
            colors.disabledColor = ButtonDisabled;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }
    }

    public static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent != null ? parent.gameObject.layer : 5;
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    public static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    public static Image CreateImage(string name, Transform parent, Color color, bool roundedCorners = true, bool raycast = false)
    {
        RectTransform rect = CreateRect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = raycast;
        if (roundedCorners)
        {
            image.sprite = Rounded;
            image.type = Image.Type.Sliced;
        }

        return image;
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool wrap = false)
    {
        RectTransform rect = CreateRect(name, parent);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        label.overflowMode = wrap ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
        return label;
    }

    public static Button CreateButton(string name, Transform parent, string label, UnityAction onClick, float height = 54f, float fontSize = 24f)
    {
        Image background = CreateImage(name, parent, Color.white, true, true);
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.colors = ButtonColors;
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        LayoutElement layout = background.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;

        TextMeshProUGUI text = CreateText("Label", background.transform, label, fontSize, Text, TextAlignmentOptions.Center);
        Stretch(text.rectTransform, 12f, 4f, 12f, 4f);
        return button;
    }

    public static TextMeshProUGUI GetLabel(Button button)
    {
        return button != null ? button.GetComponentInChildren<TextMeshProUGUI>(true) : null;
    }

    public static VerticalLayoutGroup AddVerticalLayout(GameObject go, float spacing, RectOffset padding, bool expandHeight = false)
    {
        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = expandHeight;
        return layout;
    }

    public static LayoutElement SetLayoutHeight(Component component, float height, float flexible = 0f)
    {
        LayoutElement layout = component.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = component.gameObject.AddComponent<LayoutElement>();
        }

        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = flexible;
        return layout;
    }

    /// <summary>Label on the left, control area on the right. Returns the control area.</summary>
    public static RectTransform CreateSettingRow(Transform parent, string label, out TextMeshProUGUI valueLabel)
    {
        RectTransform row = CreateRect(label + "Row", parent);
        SetLayoutHeight(row, 52f);

        TextMeshProUGUI text = CreateText("Label", row, label, 24f, Text);
        text.rectTransform.anchorMin = new Vector2(0f, 0f);
        text.rectTransform.anchorMax = new Vector2(0.42f, 1f);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;

        RectTransform control = CreateRect("Control", row);
        control.anchorMin = new Vector2(0.42f, 0f);
        control.anchorMax = new Vector2(0.86f, 1f);
        control.offsetMin = new Vector2(0f, 10f);
        control.offsetMax = new Vector2(0f, -10f);

        valueLabel = CreateText("Value", row, string.Empty, 22f, TextDim, TextAlignmentOptions.MidlineRight);
        valueLabel.rectTransform.anchorMin = new Vector2(0.86f, 0f);
        valueLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
        valueLabel.rectTransform.offsetMin = Vector2.zero;
        valueLabel.rectTransform.offsetMax = Vector2.zero;
        return control;
    }

    public static Slider CreateSlider(Transform parent, float min, float max, float value, UnityAction<float> onChanged)
    {
        RectTransform root = CreateRect("Slider", parent);
        Stretch(root);
        Slider slider = root.gameObject.AddComponent<Slider>();

        Image track = CreateImage("Track", root, Inset, true, true);
        track.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        track.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        track.rectTransform.sizeDelta = new Vector2(0f, 10f);

        RectTransform fillArea = CreateRect("Fill Area", root);
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.sizeDelta = new Vector2(-16f, 10f);
        Image fill = CreateImage("Fill", fillArea, AccentDim, true);
        fill.rectTransform.sizeDelta = new Vector2(16f, 0f);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);

        RectTransform handleArea = CreateRect("Handle Slide Area", root);
        Stretch(handleArea, 10f, 0f, 10f, 0f);
        Image handle = CreateImage("Handle", handleArea, Color.white, true, true);
        handle.rectTransform.sizeDelta = new Vector2(22f, 0f);
        handle.rectTransform.anchorMin = new Vector2(0f, 0f);
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        ColorBlock colors = ButtonColors;
        colors.normalColor = Text;
        colors.highlightedColor = Accent;
        colors.selectedColor = Accent;
        colors.pressedColor = Accent;
        slider.colors = colors;
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(value);
        if (onChanged != null)
        {
            slider.onValueChanged.AddListener(onChanged);
        }

        return slider;
    }

    public static Toggle CreateToggle(Transform parent, bool value, UnityAction<bool> onChanged)
    {
        RectTransform root = CreateRect("Toggle", parent);
        root.anchorMin = new Vector2(0f, 0.5f);
        root.anchorMax = new Vector2(0f, 0.5f);
        root.pivot = new Vector2(0f, 0.5f);
        root.sizeDelta = new Vector2(64f, 32f);
        Toggle toggle = root.gameObject.AddComponent<Toggle>();

        Image background = CreateImage("Background", root, Color.white, true, true);
        Stretch(background.rectTransform);
        Image knob = CreateImage("Checkmark", background.transform, Accent, true);
        Stretch(knob.rectTransform, 5f, 5f, 5f, 5f);

        toggle.targetGraphic = background;
        toggle.graphic = knob;
        toggle.colors = ButtonColors;
        toggle.toggleTransition = Toggle.ToggleTransition.None;
        toggle.SetIsOnWithoutNotify(value);
        if (onChanged != null)
        {
            toggle.onValueChanged.AddListener(onChanged);
        }

        return toggle;
    }

    /// <summary>Thin accent outline drawn behind a panel's content.</summary>
    public static Image AddOutline(RectTransform panel, Color color, float thickness = 2f)
    {
        Image outline = CreateImage("Outline", panel, color, true);
        Stretch(outline.rectTransform, -thickness, -thickness, -thickness, -thickness);
        outline.transform.SetAsFirstSibling();
        outline.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        return outline;
    }

    public static string FormatTime(float seconds)
    {
        TimeSpan span = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return span.TotalHours >= 1
            ? string.Format("{0}:{1:00}:{2:00}", (int)span.TotalHours, span.Minutes, span.Seconds)
            : string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
    }

    static Sprite BuildRoundedSprite(int size, int radius)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "UiKitRounded";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Max(radius - (x + 0.5f), (x + 0.5f) - (size - radius)));
                float dy = Mathf.Max(0f, Mathf.Max(radius - (y + 0.5f), (y + 0.5f) - (size - radius)));
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                byte alpha = (byte)(Mathf.Clamp01(radius - distance + 0.5f) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.name = "UiKitRounded";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
