using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base for overlay panels managed by GameMenu (pause, inventory, journal, settings, confirm).
/// Panels build their own UI at runtime under the GameMenu overlay.
/// </summary>
public abstract class MenuPanel : MonoBehaviour
{
    protected GameMenu Menu { get; private set; }
    protected RectTransform Root { get; private set; }

    /// <summary>Control to focus when opened with a gamepad.</summary>
    public virtual Selectable FirstSelectable => null;

    public void Initialize(GameMenu menu, Vector2 size)
    {
        Menu = menu;
        Root = GetComponent<RectTransform>();
        UiKit.Place(Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);

        Image background = gameObject.AddComponent<Image>();
        background.sprite = UiKit.Rounded;
        background.type = Image.Type.Sliced;
        background.color = UiKit.Panel;
        background.raycastTarget = true;
        UiKit.AddOutline(Root, UiKit.AccentDim);

        Build();
    }

    protected abstract void Build();

    /// <summary>Called every time the panel becomes the top of the menu stack.</summary>
    public virtual void OnShow()
    {
    }

    /// <summary>"Back" when returning to another menu, "Close" when returning straight to gameplay.</summary>
    protected string BackLabel => Menu != null && Menu.Depth > 1 ? "Back" : "Close";
}
