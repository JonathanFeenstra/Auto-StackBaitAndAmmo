/* Auto-Stack Bait
 *
 * SMAPI mod that automatically adds any new bait to fishing poles that
 * have the same type of bait attached.
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

namespace AutoStackBait;

internal sealed class ModEntry : Mod
{
    private const int BaitCategory = -21;
    
    public override void Entry(IModHelper helper)
    {
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
        helper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        foreach (var addedItem in e.Added)
        {
            if (addedItem.Category != BaitCategory) continue;
            AddBaitToRods(Game1.player.Items, addedItem);
        }
    }
    
    private static void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        foreach (var addedItem in e.Added)
        {
            if (addedItem.Category != BaitCategory) continue;
            AddBaitToRods(e.Chest.Items, addedItem);
        }
    }

    private static void AddBaitToRods(Inventory inventory, Item newBait)
    {
        foreach (var inventoryItem in inventory)
        {
            if (inventoryItem is not FishingRod rod) continue;
            var currentBait = rod.GetBait();
            if (currentBait?.ItemId != newBait.ItemId) continue;
            var newStackSize = currentBait.Stack + newBait.Stack;
            var maxSize = currentBait.maximumStackSize();
            if (newStackSize > maxSize)
            {
                currentBait.Stack = maxSize;
                newBait.Stack = newStackSize - maxSize;
            }
            else
            {
                currentBait.Stack = newStackSize;
                inventory.RemoveButKeepEmptySlot(newBait);
                return;
            }
        }
    }
}