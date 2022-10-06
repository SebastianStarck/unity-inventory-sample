using System;
using System.Collections.Generic;
using System.Linq;
using Code.Inventory;

#nullable enable
namespace Inventory
{
    [Serializable]
    /*
     * TODO: Add safe array access for inventory
     */
    internal class Inventory
    {
        public int size = 18;
        private readonly Item?[] _inventory = new Item[18];
        private HashSet<int> _emptySlots;
        public Item?[] Items => _inventory.ToArray();

        // TODO: Add dynamic inventory size
        public Inventory()
        {
            _emptySlots = Enumerable.Range(0, size).ToHashSet();
        }

        /// <summary>
        /// Check if there is at least one empty slot available.
        /// </summary>
        /// <returns></returns>
        public bool HasFreeSlot() => _emptySlots.Any();

        /// <summary>
        /// Stores a collection of items in the inventory.
        /// </summary>
        /// <param name="items"></param>
        public void StoreMany(IEnumerable<Item> items)
        {
            // TODO: check if successful?
            foreach (var item in items) StoreItem(item);
        }

        /// <summary>
        /// Stores an item in the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Returns the used slot number.
        /// Returns -1 if the op failed.</returns>
        public int StoreItem(Item? item)
        {
            if (item is null) return -1;

            // FIXME: Maybe sorting should be executed somewhere else  
            _emptySlots = _emptySlots.OrderBy(x => x).ToHashSet();
            if (!_emptySlots.Any()) return -1;

            var emptySlot = _emptySlots.First();
            _inventory[emptySlot] = item;
            _emptySlots.Remove(emptySlot);
            item.InventoryPosition = emptySlot;

            return emptySlot;
        }

        /// <summary>
        /// Stores an item in a slot position, swapping in with the slot's current one if possible.
        /// </summary>
        /// <param name="item">Item to be put in slot</param>
        /// <param name="position">Slot position</param>
        /// <returns>Tuple of original item and the previously in the given slot</returns>
        public Tuple<Item?, Item?> SwapItemInPosition(Item? item, int position)
        {
            var otherItem = _inventory[position];

            if (item is null && otherItem is null) return new Tuple<Item?, Item?>(default, default);

            // FIXME: Handle item null case scenario
            if (item is not null)
            {
                var itemPosition = item.InventoryPosition;
                item.InventoryPosition = position;

                if (otherItem is not null) otherItem.InventoryPosition = itemPosition;
                else _emptySlots.Add(position);
            }
            // FIXME: Isn't this basically removing the item?
            else _emptySlots.Add(position);

            return new Tuple<Item?, Item?>(item, otherItem);
        }

        /// <summary>
        /// Stores an item in a slot position. The op will be not be completed on occupied slots unless the force flag is enabled
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public bool StoreItemInPosition(Item? item, int position, bool force = false)
        {
            if (!_emptySlots.Contains(position) && !force) return false;

            _inventory[position] = item;

            if (item is not null)
            {
                item.InventoryPosition = position;
                _emptySlots.Remove(position);
            }
            else
            {
                // FIXME: Overkill?
                _emptySlots.Add(position);
            }

            return true;
        }

        /// <summary>
        /// Checks if the inventory has a given position available.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool SlotPositionIsFree(int position) => _emptySlots.Contains(position);

        /// <summary>
        /// Check if the inventory has a given position available.
        /// </summary>
        /// <param name="slotPosition">Out int of the first available slot position</param>
        /// <returns></returns>
        public bool HasFreeSlot(out int slotPosition)
        {
            slotPosition = _emptySlots.Any() ? _emptySlots.OrderBy(x => x).First() : -1;

            return slotPosition != -1;
        }

        /// <summary>
        /// Fetches an item from the first matching slot inventory. Does NOT remove the item from the slot.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Item in slot</returns>
        // FIXME: This method is WEIRD
        public Item? FetchItem(Item item) => _inventory.FirstOrDefault(storedItem => storedItem == item);

        /// <summary>
        /// Fetches an item from a slot position. Does NOT remove the item from the slot.
        /// </summary>
        /// <param name="position">Slot position</param>
        /// <returns>Item in slot</returns>
        public Item? FetchItemByPosition(int position) => _inventory[position];

        /// <summary>
        /// Removes an item from the given slot position
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Item in slot</returns>
        public Item? RemoveItemFromSlot(int position)
        {
            if (SlotPositionIsFree(position)) return default;

            var item = Items[position];
            Items[position] = null;
            _emptySlots.Add(position);

            return item;
        }

        /// <summary>
        /// Swaps items between slots
        /// </summary>
        /// <param name="a">Slot position</param>
        /// <param name="b">Other slot position</param>
        /// <returns>Returns a tuple of &lt;itemA, itemB&gt;</returns> 
        public Tuple<Item?, Item?> SwapSlotItems(int a, int b)
        {
            var itemA = _inventory[a];
            var itemB = _inventory[b];

            _inventory[a] = itemB;
            _inventory[b] = itemA;

            if (itemB is null) _emptySlots.Add(a);
            else _emptySlots.Remove(a);

            if (itemA is null) _emptySlots.Add(b);
            else _emptySlots.Remove(b);

            return new Tuple<Item?, Item?>(itemA, itemB);
        }
    }
}
#nullable disable