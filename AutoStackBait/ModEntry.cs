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
using StardewValley.Tools;

namespace AutoStackBait;

internal sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
    }
    
    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        const int baitCategory = -21;
        foreach (var addedItem in e.Added)
        {
            if (addedItem.Category != baitCategory) continue;
            foreach (var inventoryItem in Game1.player.Items)
            {
                if (inventoryItem is not FishingRod rod) continue;
                var currentBait = rod.GetBait();
                if (currentBait?.ItemId != addedItem.ItemId) continue;
                MoveStack(addedItem, currentBait);
            }
        }
    }

    /// <summary>
    /// Moves as many items as possible from the source to the target stack. 
    /// </summary>
    /// <param name="source"/>
    /// <param name="target"/>
    /// <remarks>More optimized than <see cref="Item.addToStack"/></remarks>
    private static void MoveStack(Item source, Item target)
    {
        target.Stack += source.Stack;
        var maxSize = target.maximumStackSize();
        if (target.Stack > maxSize)
        {
            source.Stack = target.Stack - maxSize; 
            target.Stack = maxSize;
        }
        else
        {
            Game1.player.Items.Remove(source);
        }
    }
}