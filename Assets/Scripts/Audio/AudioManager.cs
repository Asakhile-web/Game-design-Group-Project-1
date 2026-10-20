using System;
using UnityEngine;

/// <summary>
/// Named clips for teammates: add entries in the Inspector, then call AudioManager.Play("pickup").
/// Master / music / SFX volumes come from GameSettings (PlayerPrefs).
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Serializable]
    public struct NamedClip
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [SerializeField] NamedClip[] clips;
    [SerializeField] AudioClip musicLoop;
    [SerializeField] AudioClip ambientLoop;
    [SerializeField] int sfxVoices = 8;

    static AudioManager instance;
    AudioSource[] sfx;
    AudioSource music;
    AudioSource ambient;
    int voice;

    public static AudioManager Instance => instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildSources();
        GameSettings.Changed += ApplyVolumes;
        ApplyVolumes();
        PlayLoops();
    }

    void OnDestroy()
    {
        GameSettings.Changed -= ApplyVolumes;
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Play(string name)
    {
        if (instance == null || string.IsNullOrEmpty(name))
        {
            return;
        }

        instance.PlayNamed(name);
    }

    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (instance == null || clip == null)
        {
            return;
        }

        instance.PlayOneShot(clip, volume);
    }

    public void PlayNamed(string cueName)
    {
        if (clips == null)
        {
            return;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].clip != null && string.Equals(clips[i].name, cueName, StringComparison.OrdinalIgnoreCase))
            {
                float vol = clips[i].volume > 0.01f ? clips[i].volume : 1f;
                PlayOneShot(clips[i].clip, vol);
                return;
            }
        }
    }

    void PlayOneShot(AudioClip clip, float volume)
    {
        if (sfx == null || sfx.Length == 0)
        {
            return;
        }

        AudioSource source = sfx[voice];
        voice = (voice + 1) % sfx.Length;
        source.pitch = 1f;
        source.PlayOneShot(clip, Mathf.Clamp01(volume) * GameSettings.SfxVolume);
    }

    void BuildSources()
    {
        int voices = Mathf.Max(2, sfxVoices);
        sfx = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            sfx[i] = CreateSource("Sfx" + i, loop: false);
        }

        music = CreateSource("Music", loop: true);
        ambient = CreateSource("Ambient", loop: true);
    }

    AudioSource CreateSource(string objectName, bool loop)
    {
        GameObject child = new GameObject(objectName);
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    void PlayLoops()
    {
        if (musicLoop != null)
        {
            music.clip = musicLoop;
            music.Play();
        }

        if (ambientLoop != null)
        {
            ambient.clip = ambientLoop;
            ambient.Play();
        }
        else
        {
            PlayNamed(AudioCue.Ambient);
        }
    }

    void ApplyVolumes()
    {
        AudioListener.volume = GameSettings.MasterVolume;
        if (music != null)
        {
            music.volume = GameSettings.MusicVolume;
        }

        if (ambient != null)
        {
            ambient.volume = GameSettings.SfxVolume * 0.6f;
        }
    }

    public static AudioManager Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("AudioManager");
        return go.AddComponent<AudioManager>();
    }
}
