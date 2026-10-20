using UnityEngine;

/// <summary>
/// Pulses emission on the renderer currently under the interaction ray. Caches per collider.
/// </summary>
public class InteractableHighlight : MonoBehaviour
{
    [SerializeField] InteractionRaycaster raycaster;
    [SerializeField] Color pulseColor = new Color(0.95f, 0.72f, 0.28f, 1f);
    [SerializeField] float pulseSpeed = 2.5f;
    [SerializeField] float pulseStrength = 0.45f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    Collider cachedCollider;
    Renderer cachedRenderer;
    MaterialPropertyBlock block;
    Color restColor;
    bool hasEmission;

    public InteractionRaycaster Raycaster
    {
        get => raycaster;
        set => raycaster = value;
    }

    void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        Collider current = raycaster != null && raycaster.HasTarget ? raycaster.CurrentCollider : null;
        if (current != cachedCollider)
        {
            Restore();
            cachedCollider = current;
            Cache(current);
        }

        if (cachedRenderer == null)
        {
            return;
        }

        float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f) * pulseStrength;
        Color color = Color.Lerp(restColor, pulseColor, pulse);
        cachedRenderer.GetPropertyBlock(block);
        if (hasEmission)
        {
            block.SetColor(EmissionColor, color);
        }
        else
        {
            block.SetColor(BaseColor, color);
            block.SetColor("_Color", color);
        }

        cachedRenderer.SetPropertyBlock(block);
    }

    void OnDisable()
    {
        Restore();
    }

    void Cache(Collider col)
    {
        cachedRenderer = null;
        hasEmission = false;
        if (col == null)
        {
            return;
        }

        cachedRenderer = col.GetComponentInParent<Renderer>();
        if (cachedRenderer == null)
        {
            return;
        }

        Material mat = cachedRenderer.sharedMaterial;
        hasEmission = mat != null && mat.HasProperty(EmissionColor);
        if (hasEmission)
        {
            restColor = mat.GetColor(EmissionColor);
        }
        else
        {
            restColor = mat != null && mat.HasProperty(BaseColor) ? mat.GetColor(BaseColor) : Color.white;
        }
    }

    void Restore()
    {
        if (cachedRenderer == null || block == null)
        {
            cachedCollider = null;
            cachedRenderer = null;
            return;
        }

        cachedRenderer.GetPropertyBlock(block);
        if (hasEmission)
        {
            block.SetColor(EmissionColor, restColor);
        }
        else
        {
            block.SetColor(BaseColor, restColor);
            block.SetColor("_Color", restColor);
        }

        cachedRenderer.SetPropertyBlock(block);
        cachedCollider = null;
        cachedRenderer = null;
    }
}
