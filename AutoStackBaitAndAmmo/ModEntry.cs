/* Auto-Stack Bait & Ammo
 *
 * SMAPI mod that automatically adds any new bait to fishing poles that
 * have the same type of bait attached and new ammo to slingshots that
 * have the same type of ammo attached.
 * 
 * Copyright (C) 2024 Jonathan Feenstra
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Tools;

namespace AutoStackBaitAndAmmo;

internal sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
        helper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        foreach (var addedItem in e.Added)
        {
            if (addedItem.Category == StardewValley.Object.baitCategory)
            {
                AddBaitToRods(Game1.player.Items, addedItem);
            }
            else if (IsSlingshotAmmo(addedItem))
            {
                AddAmmoToSlingshots(Game1.player.Items, addedItem);
            }
        }
    }
    
    private static void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        foreach (var addedItem in e.Added)
        {
            if (addedItem.Category == StardewValley.Object.baitCategory)
            {
                AddBaitToRods(e.Chest.Items, addedItem);
            }
            else if (IsSlingshotAmmo(addedItem))
            {
                AddAmmoToSlingshots(e.Chest.Items, addedItem);
            }
        }
    }

    private static void AddBaitToRods(Inventory inventory, Item bait)
    {
        foreach (var item in inventory)
        {
            if (item is not FishingRod rod) continue;
            var currentBait = rod.GetBait();
            if (currentBait is null || !AreBaitTypesEqual(currentBait, bait)) continue;
            if (StackItems(inventory, currentBait, bait)) return;
        }
    }

    private static void AddAmmoToSlingshots(Inventory inventory, Item ammo)
    {
        foreach (var item in inventory)
        {
            if (item is not Slingshot slingshot) continue;
            var currentAmmo = slingshot.attachments[0];
            if (currentAmmo?.ItemId != ammo.ItemId) continue;
            if (StackItems(inventory, currentAmmo, ammo)) return;
        }
    }
    
    // returns true if the new item stack is fully transferred to the old item
    private static bool StackItems(Inventory inventory, Item oldItem, Item newItem)
    {
        var newStackSize = oldItem.Stack + newItem.Stack;
        var maxSize = oldItem.maximumStackSize();
        if (newStackSize > maxSize)
        {
            oldItem.Stack = maxSize;
            newItem.Stack = newStackSize - maxSize;
            return false;
        }

        oldItem.Stack = newStackSize;
        inventory.RemoveButKeepEmptySlot(newItem);
        return true;
    }

    // based on Slingshot.canThisBeAttached
    private static bool IsSlingshotAmmo(Item item)
    {
        switch (item.QualifiedItemId)
        {
            case "(O)378":
            case "(O)380":
            case "(O)382":
            case "(O)384":
            case "(O)386":
            case "(O)388":
            case "(O)390":
            case "(O)441":
                return true;
            default:
                if (item is not StardewValley.Object obj || obj.bigCraftable.Value)
                {
                    return false;
                }

                return item.Category is -5 or -79 or -75;
        }
    }
    
    private static bool AreBaitTypesEqual(Item bait1, Item bait2) =>
        bait1.QualifiedItemId == bait2.QualifiedItemId && bait1.Name == bait2.Name;
}