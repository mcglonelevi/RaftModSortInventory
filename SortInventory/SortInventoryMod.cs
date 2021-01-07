using System;
using System.Collections.Generic;
using UnityEngine;

namespace SortInventory
{
    public class SortInventoryMod : Mod
    {
        private void Start()
        {
            Debug.Log("SortInventory loaded!");
        }

        public void Update()
        {   
            if (CanvasHelper.ActiveMenu == MenuType.Inventory && Input.GetKeyDown(KeyCode.Z)) {
                Network_Player player = RAPI.GetLocalPlayer();
                if (player != null) {
                    SortPlayerInventory(player);
                }
            }
        }

        private void SortPlayerInventory(Network_Player player) {
            PlayerInventory inv = player.Inventory;
        
            // Get all of the inventory slots that are not in the hotbar.
            List<Slot> nonHotbar = inv.allSlots.GetRange(player.Inventory.hotslotCount, player.Inventory.allSlots.Count - player.Inventory.hotslotCount);

            Sort(nonHotbar);

            if (inv.secondInventory != null)
            {
                Sort(inv.secondInventory.allSlots);
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
            
            tempInstances.Sort((x, y) => String.Compare(x.settings_Inventory.DisplayName, y.settings_Inventory.DisplayName, StringComparison.Ordinal));

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
                    Slot toSlot = this.FindSuitableSlot(nonHotbar, i, currSlot.GetItemBase());
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

        private Slot FindSuitableSlot(List<Slot> inv, int currIndex, Item_Base stackableItem = null)
        {
            bool flag = stackableItem != null && stackableItem.settings_Inventory.Stackable;
            for (var i = 0; i < currIndex; i++)
            {
                Slot slot2 = inv[i];
                if (slot2.IsEmpty)
                {
                    return slot2;
                }
                if (flag && !slot2.StackIsFull() && !slot2.IsEmpty && slot2.itemInstance.UniqueIndex == stackableItem.UniqueIndex)
                {
                    return slot2;
                }
            }
            return null;
        }

        public void OnModUnload()
        {
            Debug.Log("SortInventory has been unloaded!");
        }
    }
}