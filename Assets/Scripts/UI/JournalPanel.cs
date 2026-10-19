using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clue journal: discovered clues listed on the left, the selected entry on the right.
/// Reads straight from ClueJournal, so entries persist between openings.
/// </summary>
public class JournalPanel : MenuPanel
{
    readonly List<Button> entryButtons = new List<Button>();

    RectTransform listContent;
    TextMeshProUGUI emptyLabel;
    TextMeshProUGUI entryTitle;
    TextMeshProUGUI entryBody;
    TextMeshProUGUI countLabel;
    Button closeButton;
    ClueJournal subscribed;
    int selectedIndex = -1;

    public override Selectable FirstSelectable => entryButtons.Count > 0 ? entryButtons[0] : closeButton;

    protected override void Build()
    {
        UiKit.AddVerticalLayout(gameObject, 16f, new RectOffset(48, 48, 36, 36));

        TextMeshProUGUI title = UiKit.CreateText("Title", Root, "Journal", 44f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(title, 56f);

        countLabel = UiKit.CreateText("Count", Root, string.Empty, 20f, UiKit.TextDim, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(countLabel, 26f);

        RectTransform body = UiKit.CreateRect("Body", Root);
        UiKit.SetLayoutHeight(body, 400f, 1f);

        Image listBackground = UiKit.CreateImage("List", body, UiKit.Inset, true);
        listBackground.rectTransform.anchorMin = new Vector2(0f, 0f);
        listBackground.rectTransform.anchorMax = new Vector2(0.36f, 1f);
        listBackground.rectTransform.offsetMin = Vector2.zero;
        listBackground.rectTransform.offsetMax = new Vector2(-8f, 0f);
        listContent = BuildScrollList(listBackground.rectTransform);

        emptyLabel = UiKit.CreateText("Empty", listBackground.transform,
            "No clues yet.\nExamine objects around the study to record what you find.",
            20f, UiKit.TextDim, TextAlignmentOptions.Center, wrap: true);
        UiKit.Stretch(emptyLabel.rectTransform, 20f, 20f, 20f, 20f);

        Image page = UiKit.CreateImage("Entry", body, UiKit.Inset, true);
        page.rectTransform.anchorMin = new Vector2(0.36f, 0f);
        page.rectTransform.anchorMax = new Vector2(1f, 1f);
        page.rectTransform.offsetMin = new Vector2(8f, 0f);
        page.rectTransform.offsetMax = Vector2.zero;
        UiKit.AddVerticalLayout(page.gameObject, 12f, new RectOffset(28, 28, 24, 24));
        entryTitle = UiKit.CreateText("EntryTitle", page.transform, string.Empty, 32f, UiKit.Accent, TextAlignmentOptions.TopLeft, wrap: true);
        UiKit.SetLayoutHeight(entryTitle, 44f);
        entryBody = UiKit.CreateText("EntryBody", page.transform, string.Empty, 23f, UiKit.Text, TextAlignmentOptions.TopLeft, wrap: true);
        UiKit.SetLayoutHeight(entryBody, 200f, 1f);

        closeButton = UiKit.CreateButton("Close", Root, "Close", () => Menu.Back());
    }

    static RectTransform BuildScrollList(RectTransform parent)
    {
        RectTransform viewport = UiKit.CreateRect("Viewport", parent);
        UiKit.Stretch(viewport, 10f, 10f, 10f, 10f);
        viewport.gameObject.AddComponent<RectMask2D>();
        Image hitArea = viewport.gameObject.AddComponent<Image>();
        hitArea.color = new Color(0f, 0f, 0f, 0f);

        RectTransform content = UiKit.CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = Vector2.zero;
        UiKit.AddVerticalLayout(content.gameObject, 8f, new RectOffset(0, 0, 0, 0));
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = parent.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        return content;
    }

    void OnDisable()
    {
        if (subscribed != null)
        {
            subscribed.ClueAdded -= OnClueAdded;
            subscribed = null;
        }
    }

    public override void OnShow()
    {
        ClueJournal journal = Menu.Journal;
        if (journal != subscribed)
        {
            if (subscribed != null)
            {
                subscribed.ClueAdded -= OnClueAdded;
            }

            subscribed = journal;
            if (subscribed != null)
            {
                subscribed.ClueAdded += OnClueAdded;
            }
        }

        UiKit.GetLabel(closeButton).text = BackLabel;
        Rebuild();
    }

    void OnClueAdded(ClueDefinition clue)
    {
        Rebuild();
    }

    void Rebuild()
    {
        ClueJournal journal = Menu.Journal;
        int count = journal != null ? journal.Count : 0;

        while (entryButtons.Count < count)
        {
            int index = entryButtons.Count;
            Button button = UiKit.CreateButton("Clue" + (index + 1), listContent, string.Empty, () => Select(index), 48f, 21f);
            TextMeshProUGUI label = UiKit.GetLabel(button);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            entryButtons.Add(button);
        }

        for (int i = 0; i < entryButtons.Count; i++)
        {
            bool visible = i < count;
            entryButtons[i].gameObject.SetActive(visible);
            if (visible)
            {
                UiKit.GetLabel(entryButtons[i]).text = journal.Clues[i].Title;
            }
        }

        emptyLabel.gameObject.SetActive(count == 0);
        countLabel.text = count == 1 ? "1 clue recorded" : count + " clues recorded";

        if (count == 0)
        {
            selectedIndex = -1;
        }
        else if (selectedIndex < 0 || selectedIndex >= count)
        {
            selectedIndex = count - 1;
        }

        ShowEntry();
    }

    void Select(int index)
    {
        selectedIndex = index;
        ShowEntry();
    }

    void ShowEntry()
    {
        ClueJournal journal = Menu.Journal;
        for (int i = 0; i < entryButtons.Count; i++)
        {
            UiKit.GetLabel(entryButtons[i]).color = i == selectedIndex ? UiKit.Accent : UiKit.Text;
        }

        if (journal == null || selectedIndex < 0 || selectedIndex >= journal.Count)
        {
            entryTitle.text = string.Empty;
            entryBody.text = string.Empty;
            return;
        }

        ClueDefinition clue = journal.Clues[selectedIndex];
        entryTitle.text = clue.Title;
        entryBody.text = string.IsNullOrEmpty(clue.Description) ? "No notes." : clue.Description;
    }
}
