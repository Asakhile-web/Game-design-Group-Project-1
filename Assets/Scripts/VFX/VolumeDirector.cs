using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Runtime URP Volume: vignette, bloom, colour grading. Pulses vignette on low time / puzzle solve.
/// </summary>
public class VolumeDirector : MonoBehaviour
{
    static VolumeDirector instance;

    Volume volume;
    Vignette vignette;
    float pulse;
    float baseVignette = 0.28f;

    public static void Pulse(float amount)
    {
        if (instance == null)
        {
            return;
        }

        instance.pulse = Mathf.Max(instance.pulse, amount);
    }

    void Awake()
    {
        instance = this;
        Build();
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
        if (vignette == null)
        {
            return;
        }

        float extra = 0f;
        GameFlow flow = GameFlow.Instance;
        if (flow != null && flow.IsLowTime)
        {
            extra = (Mathf.Sin(Time.unscaledTime * 4f) * 0.5f + 0.5f) * 0.22f;
        }

        pulse = Mathf.MoveTowards(pulse, 0f, Time.unscaledDeltaTime);
        vignette.intensity.value = Mathf.Clamp01(baseVignette + extra + pulse);
    }

    void Build()
    {
        volume = GetComponent<Volume>();
        if (volume == null)
        {
            volume = gameObject.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 1f;
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.hideFlags = HideFlags.HideAndDontSave;
        volume.profile = profile;

        vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(baseVignette);
        vignette.smoothness.Override(0.45f);
        vignette.color.Override(new Color(0.08f, 0.04f, 0.02f));

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.18f);
        bloom.threshold.Override(0.9f);

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.saturation.Override(8f);
        color.contrast.Override(8f);
        color.postExposure.Override(-0.15f);
        color.colorFilter.Override(new Color(1f, 0.95f, 0.88f));
    }
}
