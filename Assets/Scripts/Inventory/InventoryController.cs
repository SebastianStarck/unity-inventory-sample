using System;
using System.Collections.Generic;
using System.Linq;
using Generic;
using Inventory;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.Inventory
{
    /// <summary>
    /// Designated partial for logical control and access to the Item Database / Inventory Data
    /// </summary>
    public partial class InventoryController
    {
        private static readonly Dictionary<string, Item> ItemDatabase = new();
        [SerializeField] private global::Inventory.Inventory inventory = new();
        private Dictionary<EquipmentPart, Item> _playerEquipment = new();
        [SerializeField] private Item[] equippedItems;

        /// <summary>
        /// Retrieve item details based on the GUID
        /// </summary>
        /// <param name="guid">ID to look up</param>
        /// <returns>Item details</returns>
        public static Item GetItemByGuid(string guid) => ItemDatabase.ContainsKey(guid) ? ItemDatabase[guid] : default;

        private void Awake()
        {
            // TODO: Move database to another class
            PopulateDatabase();
            InitUI();
        }

        // FIXME: Replace me with an external database system
        private void PopulateDatabase()
        {
            foreach (var part in Enum<EquipmentPart>.Values)
            {
                ItemFactory.GetMany(2, part).ForEach(item => ItemDatabase.Add(item.GUID, item));
            }
        }

        private void Start()
        {
            inventory.StoreMany(ItemDatabase.Values);
            UIUpdateInventorySlots();
        }

        /// <summary>
        /// Equips the item held by a slot.
        /// The op will fail if there is no item or the item is not equipable 
        /// </summary>
        /// <param name="slot"></param>
        private void EquipItemInSlot(InventorySlot slot)
        {
            if (slot.Item?.Part == null) return;

            var part = (EquipmentPart)slot.Item.Part;
            _playerEquipment.TryGetValue(part, out var equippedItem);
            _playerEquipment[part] = slot.Item;

            inventory.StoreItemInPosition(equippedItem, slot.SlotPosition, force: true);
            
            // TODO: Update affected slots only
            UIUpdateInventorySlots();
            UIUpdateEquipmentSlots();
            equippedItems = _playerEquipment.Values.ToArray();
        }

        /// <summary>
        /// Unequips the item hold by an equipment slot.
        /// The op will fail if there are is no available space.
        /// </summary>
        /// <param name="slot"></param>
        private void UnequipItemInSlot(InventorySlot slot)
        {
            if (!inventory.HasFreeSlot() || slot.Item is null) return;

            var item = slot.Item;
            slot.HoldItem(null);
            _playerEquipment.Remove((EquipmentPart)slot.Part);
            inventory.StoreItem(item);

            UIUpdateInventorySlots();
        }

        /// <summary>
        /// Swaps items between slots
        /// </summary>
        /// <param name="originSlot"></param>
        /// <param name="targetSlot"></param>
        public void SwapItemsInSlots(InventorySlot originSlot, InventorySlot targetSlot)
        {
            inventory.SwapSlotItems(originSlot.SlotPosition, targetSlot.SlotPosition);

            if (originSlot.IsEquipmentSlot || targetSlot.IsEquipmentSlot)
            {
                var part = (EquipmentPart)originSlot.Part;
                _playerEquipment[part] = originSlot.Item;
            }

            // TODO: Update affected slots only
            UIUpdateInventorySlots();
            // TODO: Check for equipment slots & Update affected slots only
            UIUpdateEquipmentSlots();
        }
    }
}