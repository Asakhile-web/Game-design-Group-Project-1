using System;
using UnityEngine;

/// <summary>
/// Player-facing settings persisted in PlayerPrefs and applied to the running game.
/// Only settings with a real effect live here.
/// </summary>
public static class GameSettings
{
    const string VolumeKey = "settings.masterVolume";
    const string SensitivityKey = "settings.mouseSensitivity";
    const string HeadBobKey = "settings.headBob";
    const string ShowTimerKey = "settings.showTimer";
    const string QualityKey = "settings.quality";

    public const float MinSensitivity = 0.02f;
    public const float MaxSensitivity = 0.4f;

    public static event Action Changed;

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, 1f);
        set
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, value);
            AudioListener.volume = value;
            Changed?.Invoke();
        }
    }

    public static bool HasSensitivity => PlayerPrefs.HasKey(SensitivityKey);

    public static float GetMouseSensitivity(float fallback)
    {
        return PlayerPrefs.GetFloat(SensitivityKey, fallback);
    }

    public static void SetMouseSensitivity(float value, PlayerController player)
    {
        value = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
        PlayerPrefs.SetFloat(SensitivityKey, value);
        if (player != null)
        {
            player.MouseSensitivity = value;
        }

        Changed?.Invoke();
    }

    public static bool HeadBobEnabled => PlayerPrefs.GetInt(HeadBobKey, 1) == 1;

    public static void SetHeadBob(bool enabled, PlayerController player)
    {
        PlayerPrefs.SetInt(HeadBobKey, enabled ? 1 : 0);
        HeadBob bob = FindHeadBob(player);
        if (bob != null)
        {
            bob.BobEnabled = enabled;
        }

        Changed?.Invoke();
    }

    public static bool ShowTimer
    {
        get => PlayerPrefs.GetInt(ShowTimerKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(ShowTimerKey, value ? 1 : 0);
            Changed?.Invoke();
        }
    }

    /// <summary>Fullscreen only has an effect in standalone builds, not in the Editor Game view.</summary>
    public static bool FullscreenSupported =>
        !Application.isEditor &&
        (Application.platform == RuntimePlatform.WindowsPlayer ||
         Application.platform == RuntimePlatform.OSXPlayer ||
         Application.platform == RuntimePlatform.LinuxPlayer);

    public static bool Fullscreen
    {
        get => Screen.fullScreen;
        set
        {
            Screen.fullScreen = value;
            Changed?.Invoke();
        }
    }

    public static int QualityLevel
    {
        get => QualitySettings.GetQualityLevel();
        set
        {
            int count = QualitySettings.names.Length;
            if (count == 0)
            {
                return;
            }

            value = ((value % count) + count) % count;
            PlayerPrefs.SetInt(QualityKey, value);
            QualitySettings.SetQualityLevel(value, true);
            Changed?.Invoke();
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>Applies saved values at level start.</summary>
    public static void ApplySaved(PlayerController player)
    {
        AudioListener.volume = MasterVolume;

        if (player != null && HasSensitivity)
        {
            player.MouseSensitivity = Mathf.Clamp(GetMouseSensitivity(player.MouseSensitivity), MinSensitivity, MaxSensitivity);
        }

        HeadBob bob = FindHeadBob(player);
        if (bob != null)
        {
            bob.BobEnabled = HeadBobEnabled;
        }

        if (PlayerPrefs.HasKey(QualityKey))
        {
            int saved = PlayerPrefs.GetInt(QualityKey);
            if (saved >= 0 && saved < QualitySettings.names.Length && saved != QualitySettings.GetQualityLevel())
            {
                QualitySettings.SetQualityLevel(saved, true);
            }
        }
    }

    static HeadBob FindHeadBob(PlayerController player)
    {
        if (player == null || player.CameraTransform == null)
        {
            return null;
        }

        return player.CameraTransform.GetComponent<HeadBob>();
    }
}
