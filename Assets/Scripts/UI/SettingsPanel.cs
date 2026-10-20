using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game settings. Every control changes something real and is saved via GameSettings.
/// </summary>
public class SettingsPanel : MenuPanel
{
    Slider volume;
    TextMeshProUGUI volumeValue;
    Slider music;
    TextMeshProUGUI musicValue;
    Slider sfx;
    TextMeshProUGUI sfxValue;
    Slider sensitivity;
    TextMeshProUGUI sensitivityValue;
    Toggle headBob;
    Toggle showTimer;
    Toggle fullscreen;
    GameObject fullscreenRow;
    GameObject qualityRow;
    TextMeshProUGUI qualityValue;
    Button closeButton;

    public override Selectable FirstSelectable => volume;

    protected override void Build()
    {
        UiKit.AddVerticalLayout(gameObject, 10f, new RectOffset(56, 56, 36, 36));

        TextMeshProUGUI title = UiKit.CreateText("Title", Root, "Settings", 44f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(title, 60f);

        RectTransform control = UiKit.CreateSettingRow(Root, "Master Volume", out volumeValue);
        volume = UiKit.CreateSlider(control, 0f, 1f, GameSettings.MasterVolume, value =>
        {
            GameSettings.MasterVolume = value;
            volumeValue.text = Mathf.RoundToInt(value * 100f) + "%";
        });

        control = UiKit.CreateSettingRow(Root, "Music", out musicValue);
        music = UiKit.CreateSlider(control, 0f, 1f, GameSettings.MusicVolume, value =>
        {
            GameSettings.MusicVolume = value;
            musicValue.text = Mathf.RoundToInt(value * 100f) + "%";
        });

        control = UiKit.CreateSettingRow(Root, "SFX", out sfxValue);
        sfx = UiKit.CreateSlider(control, 0f, 1f, GameSettings.SfxVolume, value =>
        {
            GameSettings.SfxVolume = value;
            sfxValue.text = Mathf.RoundToInt(value * 100f) + "%";
        });

        control = UiKit.CreateSettingRow(Root, "Mouse Sensitivity", out sensitivityValue);
        sensitivity = UiKit.CreateSlider(control, GameSettings.MinSensitivity, GameSettings.MaxSensitivity, 0.12f, value =>
        {
            GameSettings.SetMouseSensitivity(value, Menu.Player);
            sensitivityValue.text = value.ToString("0.00");
        });

        control = UiKit.CreateSettingRow(Root, "Head Bob", out _);
        headBob = UiKit.CreateToggle(control, GameSettings.HeadBobEnabled, value => GameSettings.SetHeadBob(value, Menu.Player));

        control = UiKit.CreateSettingRow(Root, "Show Timer", out _);
        showTimer = UiKit.CreateToggle(control, GameSettings.ShowTimer, value => GameSettings.ShowTimer = value);

        control = UiKit.CreateSettingRow(Root, "Fullscreen", out _);
        fullscreenRow = control.parent.gameObject;
        fullscreen = UiKit.CreateToggle(control, GameSettings.Fullscreen, value => GameSettings.Fullscreen = value);

        control = UiKit.CreateSettingRow(Root, "Graphics Quality", out qualityValue);
        qualityRow = control.parent.gameObject;
        HorizontalLayoutGroup arrows = control.gameObject.AddComponent<HorizontalLayoutGroup>();
        arrows.spacing = 10f;
        arrows.childControlWidth = true;
        arrows.childControlHeight = true;
        arrows.childForceExpandWidth = true;
        arrows.childForceExpandHeight = true;
        UiKit.CreateButton("Lower", control, "<", () => ChangeQuality(-1), 32f, 22f);
        UiKit.CreateButton("Higher", control, ">", () => ChangeQuality(1), 32f, 22f);

        RectTransform spacer = UiKit.CreateRect("Spacer", Root);
        UiKit.SetLayoutHeight(spacer, 0f, 1f);

        closeButton = UiKit.CreateButton("Close", Root, "Back", () => Menu.Back());
    }

    public override void OnShow()
    {
        float currentVolume = GameSettings.MasterVolume;
        volume.SetValueWithoutNotify(currentVolume);
        volumeValue.text = Mathf.RoundToInt(currentVolume * 100f) + "%";

        music.SetValueWithoutNotify(GameSettings.MusicVolume);
        musicValue.text = Mathf.RoundToInt(GameSettings.MusicVolume * 100f) + "%";
        sfx.SetValueWithoutNotify(GameSettings.SfxVolume);
        sfxValue.text = Mathf.RoundToInt(GameSettings.SfxVolume * 100f) + "%";

        float currentSensitivity = Menu.Player != null ? Menu.Player.MouseSensitivity : GameSettings.GetMouseSensitivity(0.12f);
        sensitivity.SetValueWithoutNotify(currentSensitivity);
        sensitivityValue.text = currentSensitivity.ToString("0.00");

        headBob.SetIsOnWithoutNotify(GameSettings.HeadBobEnabled);
        showTimer.SetIsOnWithoutNotify(GameSettings.ShowTimer);

        fullscreenRow.SetActive(GameSettings.FullscreenSupported);
        fullscreen.SetIsOnWithoutNotify(GameSettings.Fullscreen);

        qualityRow.SetActive(QualitySettings.names.Length > 1);
        RefreshQuality();

        UiKit.GetLabel(closeButton).text = BackLabel;
    }

    void ChangeQuality(int delta)
    {
        GameSettings.QualityLevel = GameSettings.QualityLevel + delta;
        RefreshQuality();
    }

    void RefreshQuality()
    {
        string[] names = QualitySettings.names;
        int level = GameSettings.QualityLevel;
        qualityValue.text = level >= 0 && level < names.Length ? names[level] : string.Empty;
    }
}
