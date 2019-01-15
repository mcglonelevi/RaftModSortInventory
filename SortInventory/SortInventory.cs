using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ModTitle("SortInventory")]
[ModDescription("A mod that will use a hotkey to sort inventory.")]
[ModAuthor("wazupwiop")]
[ModIconUrl("https://www.raftmodding.com/images/missing.jpg")]
[ModWallpaperUrl("https://www.raftmodding.com/images/missing.jpg")]
[ModVersion("0.0.1")]
[RaftVersion("Update 8 (3376123)")]
public class SortInventory : Mod
{
    private void Start()
    {
        RConsole.Log("SortInventory loaded!");
    }

    public void Update()
    {   
        // Add Your Code Here
        if (CanvasHelper.ActiveMenu == MenuType.Inventory && Input.GetKeyDown(KeyCode.Z)) {
            Network_Player player = RAPI.getLocalPlayer();
            if (player != null) {
                this.SortPlayerInventory(player);
                this.SortPlayerInventory(player);
            }
        }
    }

    private void SortPlayerInventory(Network_Player player) {
        PlayerInventory inv = player.Inventory;
        
        // Get all of the inventory slots that are not in the Hotbar.
        List<Slot> nonHotbar = inv.allSlots.GetRange(player.Inventory.hotslotCount, player.Inventory.allSlots.Count - player.Inventory.hotslotCount);

        this.Sort(nonHotbar);
        if (inv.secondInventory != null)
        {
            this.Sort(inv.secondInventory.allSlots);
        }
    }

    private void Sort(List<Slot> nonHotbar)
    {
        // Get a temporary list of all the used slots in the inventory.
        List<ItemInstance> tempInstances = new List<ItemInstance>();

        foreach (Slot slot in nonHotbar)
        {
            // Grab its amount.
            if (!slot.IsEmpty)
            {
                tempInstances.Add(slot.itemInstance.Clone());
            }
            // Reset the slot.
            slot.Reset();
        }

        tempInstances.Sort((x, y) => x.UniqueName.CompareTo(y.UniqueName));

        // Do some assignments to sort it in the inventory
        var i = 0;
        foreach (ItemInstance instance in tempInstances)
        {
            Slot currSlot = nonHotbar[i];
            currSlot.SetItem(instance);
            i++;
        }

        // Now we compress with i starting at 1
        for (i = 1; i < nonHotbar.Count; i++)
        {
            Slot currSlot = nonHotbar[i];
            if (!currSlot.IsEmpty)
            {
                Slot toSlot = this.FindSuitableSlot(nonHotbar, currSlot.GetItemBase());
                if (toSlot != currSlot && toSlot != null)
                {
                    this.TransferItems(toSlot, currSlot);
                }
            }
        }
    }

    private void TransferItems(Slot toSlot, Slot fromSlot) {
        if (!fromSlot.itemInstance.settings_Inventory.Stackable || toSlot.IsEmpty)
        {
            toSlot.SetItem(fromSlot.itemInstance);
            fromSlot.Reset();
        }
        else
        {
            int maxAmount = fromSlot.itemInstance.settings_Inventory.StackSize;
            if (fromSlot.itemInstance.Amount + toSlot.itemInstance.Amount > maxAmount) {
                int diff = maxAmount - toSlot.itemInstance.Amount;
                fromSlot.itemInstance.Amount -= diff;
                toSlot.itemInstance.Amount += diff;
                fromSlot.RefreshComponents();
                toSlot.RefreshComponents();
            } else {
                toSlot.itemInstance.Amount += fromSlot.itemInstance.Amount;
                toSlot.RefreshComponents();
                fromSlot.Reset();
            }
        }
    }

    private Slot FindSuitableSlot(List<Slot> inv, Item_Base stackableItem = null)
    {
        bool flag = stackableItem != null && stackableItem.settings_Inventory.Stackable;
        Slot slot = null;
        foreach (Slot slot2 in inv)
        {
            if (slot2.IsEmpty && slot == null)
            {
                slot = slot2;
            }
            if (flag && !slot2.StackIsFull() && !slot2.IsEmpty && slot2.itemInstance.UniqueIndex == stackableItem.UniqueIndex)
            {
                return slot2;
            }
        }
        return slot;
    }

    public void OnModUnload()
    {
        RConsole.Log("SortInventory has been unloaded!");
        Destroy(gameObject);
    }
}