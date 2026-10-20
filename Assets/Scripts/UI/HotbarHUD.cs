using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Five hotbar slots at the bottom of the existing interaction canvas.
/// </summary>
public class HotbarHUD : MonoBehaviour
{
    [SerializeField] InventorySystem inventory;
    [SerializeField] Color slotIdle = new Color(0.12f, 0.1f, 0.08f, 0.72f);
    [SerializeField] Color slotSelected = new Color(0.42f, 0.28f, 0.1f, 0.92f);
    [SerializeField] Color textIdle = new Color(0.92f, 0.86f, 0.74f, 0.85f);
    [SerializeField] Color textSelected = new Color(0.98f, 0.9f, 0.55f, 1f);

    Image[] slotBackgrounds;
    TextMeshProUGUI[] slotLabels;
    bool built;

    public InventorySystem Inventory
    {
        get => inventory;
        set
        {
            if (inventory != null)
            {
                inventory.Changed -= Refresh;
            }

            inventory = value;
            if (inventory != null && isActiveAndEnabled)
            {
                inventory.Changed += Refresh;
                Refresh();
            }
        }
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.Changed += Refresh;
        }
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.Changed -= Refresh;
        }
    }

    void Start()
    {
        EnsureUi();
        Refresh();
    }

    void Refresh()
    {
        EnsureUi();
        if (inventory == null || slotLabels == null)
        {
            return;
        }

        int selected = inventory.SelectedIndex;
        for (int i = 0; i < InventorySystem.SlotCount; i++)
        {
            ItemDefinition item = inventory.GetSlot(i);
            string name = item != null ? item.DisplayName : "—";
            slotLabels[i].text = (i + 1) + "  " + name;
            bool on = i == selected;
            slotLabels[i].color = on ? textSelected : textIdle;
            slotBackgrounds[i].color = on ? slotSelected : slotIdle;
        }
    }

    void EnsureUi()
    {
        if (built && slotLabels != null)
        {
            return;
        }

        Transform existing = transform.Find("Hotbar");
        RectTransform row;
        if (existing != null)
        {
            row = existing.GetComponent<RectTransform>();
        }
        else
        {
            GameObject rowObject = new GameObject("Hotbar", typeof(RectTransform));
            rowObject.transform.SetParent(transform, false);
            row = rowObject.GetComponent<RectTransform>();
            row.anchorMin = new Vector2(0.5f, 0f);
            row.anchorMax = new Vector2(0.5f, 0f);
            row.pivot = new Vector2(0.5f, 0f);
            row.anchoredPosition = new Vector2(0f, 36f);
            row.sizeDelta = new Vector2(920f, 56f);

            HorizontalLayoutGroup layout = rowObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(0, 0, 0, 0);
        }

        slotBackgrounds = new Image[InventorySystem.SlotCount];
        slotLabels = new TextMeshProUGUI[InventorySystem.SlotCount];

        for (int i = 0; i < InventorySystem.SlotCount; i++)
        {
            Transform child = row.Find("Slot" + (i + 1));
            GameObject slotObject;
            if (child != null)
            {
                slotObject = child.gameObject;
            }
            else
            {
                slotObject = new GameObject("Slot" + (i + 1), typeof(RectTransform), typeof(Image));
                slotObject.transform.SetParent(row, false);
            }

            Image bg = slotObject.GetComponent<Image>();
            bg.color = slotIdle;
            bg.raycastTarget = false;
            slotBackgrounds[i] = bg;

            LayoutElement layoutElement = slotObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = slotObject.AddComponent<LayoutElement>();
            }

            layoutElement.flexibleWidth = 1f;
            layoutElement.minHeight = 52f;

            Transform labelTransform = slotObject.transform.Find("Label");
            TextMeshProUGUI label;
            if (labelTransform != null)
            {
                label = labelTransform.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(slotObject.transform, false);
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 4f);
                labelRect.offsetMax = new Vector2(-8f, -4f);
                label = labelObject.GetComponent<TextMeshProUGUI>();
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.fontSize = 18f;
                label.raycastTarget = false;
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Ellipsis;
            }

            slotLabels[i] = label;
        }

        built = true;
    }
}
