using System;
using UnityEngine;

/// <summary>
/// Five-slot hotbar. Attach to the player. Using an item is selected slot + IItemUseTarget in view.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public const int SlotCount = 5;

    readonly ItemDefinition[] slots = new ItemDefinition[SlotCount];
    int selectedIndex;

    public int SelectedIndex => selectedIndex;
    public ItemDefinition SelectedItem => slots[selectedIndex];

    public event Action Changed;
    public event Action<ItemDefinition> ItemAdded;
    public event Action<ItemDefinition> ItemRemoved;
    public event Action<int> SelectionChanged;

    public ItemDefinition GetSlot(int index)
    {
        if (index < 0 || index >= SlotCount)
        {
            return null;
        }

        return slots[index];
    }

    public bool IsFull()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool Has(ItemDefinition item)
    {
        return IndexOf(item) >= 0;
    }

    public int IndexOf(ItemDefinition item)
    {
        if (item == null)
        {
            return -1;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] != null && slots[i].Matches(item))
            {
                return i;
            }
        }

        return -1;
    }

    public bool TryAdd(ItemDefinition item)
    {
        if (item == null)
        {
            return false;
        }

        int empty = -1;
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null)
            {
                empty = i;
                break;
            }
        }

        if (empty < 0)
        {
            InteractionHUD.Toast("Inventory full");
            return false;
        }

        slots[empty] = item;
        if (slots[selectedIndex] == null)
        {
            selectedIndex = empty;
            SelectionChanged?.Invoke(selectedIndex);
        }

        ItemAdded?.Invoke(item);
        Changed?.Invoke();
        InteractionHUD.Toast("Picked up " + item.DisplayName);
        Debug.Log("[InventorySystem] Added " + item.DisplayName + " to slot " + (empty + 1), this);
        return true;
    }

    public bool TryConsume(ItemDefinition item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        ItemDefinition removed = slots[index];
        slots[index] = null;
        ItemRemoved?.Invoke(removed);
        Changed?.Invoke();
        return true;
    }

    public bool TryConsumeSelected()
    {
        ItemDefinition selected = SelectedItem;
        if (selected == null)
        {
            return false;
        }

        slots[selectedIndex] = null;
        ItemRemoved?.Invoke(selected);
        Changed?.Invoke();
        return true;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= SlotCount || index == selectedIndex)
        {
            return;
        }

        selectedIndex = index;
        SelectionChanged?.Invoke(selectedIndex);
        Changed?.Invoke();
    }
}
