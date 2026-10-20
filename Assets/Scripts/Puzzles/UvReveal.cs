using TMPro;
using UnityEngine;

/// <summary>
/// Hidden writing that only shows while the UV lamp item is selected and the player is nearby.
/// </summary>
public class UvReveal : MonoBehaviour
{
    [SerializeField] ItemDefinition uvItem;
    [SerializeField] float revealRange = 4f;
    [SerializeField] CanvasGroup group;
    [SerializeField] TextMeshPro worldText;
    [SerializeField] TextMesh legacyText;

    Transform playerTransform;
    InventorySystem inventory;
    float cachedAlpha = -1f;

    public ItemDefinition UvItem
    {
        get => uvItem;
        set => uvItem = value;
    }

    void Awake()
    {
        if (group == null)
        {
            group = GetComponentInChildren<CanvasGroup>();
        }

        if (worldText == null)
        {
            worldText = GetComponentInChildren<TextMeshPro>();
        }

        if (legacyText == null)
        {
            legacyText = GetComponentInChildren<TextMesh>();
        }

        Apply(0f);
    }

    void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
            inventory = player.Inventory;
        }
    }

    void LateUpdate()
    {
        float target = 0f;
        if (playerTransform != null && uvItem != null && inventory != null)
        {
            ItemDefinition selected = inventory.SelectedItem;
            bool holding = selected != null && selected.Matches(uvItem);
            bool close = (playerTransform.position - transform.position).sqrMagnitude <= revealRange * revealRange;
            if (holding && close)
            {
                target = 1f;
            }
        }

        Apply(Mathf.MoveTowards(cachedAlpha < 0f ? 0f : cachedAlpha, target, Time.deltaTime * 4f));
    }

    void Apply(float alpha)
    {
        cachedAlpha = alpha;
        if (group != null)
        {
            group.alpha = alpha;
        }

        if (worldText != null)
        {
            Color c = worldText.color;
            c.a = alpha;
            worldText.color = c;
        }

        if (legacyText != null)
        {
            Color c = legacyText.color;
            c.a = alpha;
            legacyText.color = c;
        }
    }
}
