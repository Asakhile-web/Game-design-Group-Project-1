using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full inventory view over the paused scene: the five hotbar slots as cards plus item details.
/// Clicking a card selects that slot in InventorySystem, so the hotbar and F-to-use stay in sync.
/// </summary>
public class InventoryPanel : MenuPanel
{
    const float CardSize = 168f;

    readonly Image[] cardBackgrounds = new Image[InventorySystem.SlotCount];
    readonly Image[] cardOutlines = new Image[InventorySystem.SlotCount];
    readonly Image[] cardIcons = new Image[InventorySystem.SlotCount];
    readonly TextMeshProUGUI[] cardInitials = new TextMeshProUGUI[InventorySystem.SlotCount];
    readonly TextMeshProUGUI[] cardNames = new TextMeshProUGUI[InventorySystem.SlotCount];
    readonly Button[] cards = new Button[InventorySystem.SlotCount];

    TextMeshProUGUI detailName;
    TextMeshProUGUI detailBody;
    TextMeshProUGUI detailHint;
    Button closeButton;
    InventorySystem subscribed;

    public override Selectable FirstSelectable => cards[0];

    protected override void Build()
    {
        UiKit.AddVerticalLayout(gameObject, 16f, new RectOffset(48, 48, 36, 36));

        TextMeshProUGUI title = UiKit.CreateText("Title", Root, "Inventory", 44f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(title, 56f);

        TextMeshProUGUI subtitle = UiKit.CreateText("Subtitle", Root,
            "Select an item, then look at a lock or target and press F (or left-click) to use it.",
            20f, UiKit.TextDim, TextAlignmentOptions.Center, wrap: true);
        UiKit.SetLayoutHeight(subtitle, 30f);

        RectTransform row = UiKit.CreateRect("Slots", Root);
        UiKit.SetLayoutHeight(row, CardSize);
        HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 16f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        for (int i = 0; i < InventorySystem.SlotCount; i++)
        {
            BuildCard(row, i);
        }

        Image detail = UiKit.CreateImage("Detail", Root, UiKit.Inset, true);
        UiKit.SetLayoutHeight(detail, 150f, 1f);
        UiKit.AddVerticalLayout(detail.gameObject, 6f, new RectOffset(24, 24, 18, 18));
        detailName = UiKit.CreateText("Name", detail.transform, string.Empty, 30f, UiKit.Accent);
        UiKit.SetLayoutHeight(detailName, 40f);
        detailBody = UiKit.CreateText("Description", detail.transform, string.Empty, 22f, UiKit.Text, TextAlignmentOptions.TopLeft, wrap: true);
        UiKit.SetLayoutHeight(detailBody, 56f, 1f);
        detailHint = UiKit.CreateText("Hint", detail.transform, string.Empty, 18f, UiKit.TextDim);
        UiKit.SetLayoutHeight(detailHint, 26f);

        closeButton = UiKit.CreateButton("Close", Root, "Close", () => Menu.Back());
    }

    void BuildCard(RectTransform row, int index)
    {
        Image background = UiKit.CreateImage("Slot" + (index + 1), row, Color.white, true, true);
        background.rectTransform.sizeDelta = new Vector2(CardSize, CardSize);
        cardBackgrounds[index] = background;

        Image outline = UiKit.AddOutline(background.rectTransform, UiKit.Accent, 3f);
        outline.enabled = false;
        cardOutlines[index] = outline;

        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.colors = UiKit.ButtonColors;
        int slot = index;
        button.onClick.AddListener(() => Select(slot));
        cards[index] = button;

        TextMeshProUGUI number = UiKit.CreateText("Key", background.transform, (index + 1).ToString(), 18f, UiKit.TextDim, TextAlignmentOptions.TopLeft);
        UiKit.Stretch(number.rectTransform, 12f, 8f, 12f, 8f);

        Image icon = UiKit.CreateImage("Icon", background.transform, Color.white, false);
        UiKit.Place(icon.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f));
        icon.preserveAspect = true;
        cardIcons[index] = icon;

        TextMeshProUGUI initials = UiKit.CreateText("Initials", background.transform, string.Empty, 44f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.Place(initials.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120f, 80f));
        cardInitials[index] = initials;

        TextMeshProUGUI label = UiKit.CreateText("Name", background.transform, string.Empty, 20f, UiKit.Text, TextAlignmentOptions.Center);
        UiKit.Place(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(CardSize - 16f, 30f));
        cardNames[index] = label;
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        if (subscribed != null)
        {
            subscribed.Changed -= Refresh;
            subscribed = null;
        }
    }

    public override void OnShow()
    {
        Subscribe();
        UiKit.GetLabel(closeButton).text = BackLabel;
        Refresh();
    }

    void Subscribe()
    {
        InventorySystem inventory = Menu != null ? Menu.Inventory : null;
        if (inventory == subscribed)
        {
            return;
        }

        if (subscribed != null)
        {
            subscribed.Changed -= Refresh;
        }

        subscribed = inventory;
        if (subscribed != null)
        {
            subscribed.Changed += Refresh;
        }
    }

    void Select(int slot)
    {
        InventorySystem inventory = Menu.Inventory;
        if (inventory == null)
        {
            return;
        }

        inventory.SelectSlot(slot);
        Refresh();
    }

    void Refresh()
    {
        InventorySystem inventory = Menu != null ? Menu.Inventory : null;
        int selected = inventory != null ? inventory.SelectedIndex : -1;

        for (int i = 0; i < InventorySystem.SlotCount; i++)
        {
            ItemDefinition item = inventory != null ? inventory.GetSlot(i) : null;
            bool hasIcon = item != null && item.Icon != null;
            cardIcons[i].enabled = hasIcon;
            cardIcons[i].sprite = hasIcon ? item.Icon : null;
            cardInitials[i].text = item != null && !hasIcon ? Initials(item.DisplayName) : string.Empty;
            cardNames[i].text = item != null ? item.DisplayName : "Empty";
            cardNames[i].color = item != null ? UiKit.Text : UiKit.TextDim;
            cardOutlines[i].enabled = i == selected;
        }

        ItemDefinition current = inventory != null ? inventory.SelectedItem : null;
        if (current == null)
        {
            detailName.text = inventory != null && IsEmpty(inventory) ? "Nothing collected yet" : "Empty slot";
            detailBody.text = "Items you pick up with E appear here and in the hotbar.";
            detailHint.text = string.Empty;
            return;
        }

        detailName.text = current.DisplayName;
        detailBody.text = string.IsNullOrEmpty(current.Description) ? "No further details." : current.Description;
        detailHint.text = "Selected (slot " + (selected + 1) + ")" + (current.ConsumeOnUse ? " - used up when applied" : string.Empty);
    }

    static bool IsEmpty(InventorySystem inventory)
    {
        for (int i = 0; i < InventorySystem.SlotCount; i++)
        {
            if (inventory.GetSlot(i) != null)
            {
                return false;
            }
        }

        return true;
    }

    static string Initials(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "?";
        }

        string[] words = name.Split(' ');
        string result = string.Empty;
        for (int i = 0; i < words.Length && result.Length < 2; i++)
        {
            if (words[i].Length > 0)
            {
                result += char.ToUpperInvariant(words[i][0]);
            }
        }

        return result;
    }
}
