using System;
using System.Collections.Generic;
using Code.Inventory;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Shoddy item generator
    /// </summary>
    internal static class ItemFactory
    {
        private static SpriteCollection _icons;

        public static Item GetItem(EquipmentPart? part = null)
        {
            var name = part.ToString();
            var icon = GetIcon(name.ToLower());

            return new Item
            {
                Part = part,
                GUID = Guid.NewGuid().ToString(),
                Name = name,
                Icon = icon
            };
        }

        public static List<Item> GetMany(int amount, EquipmentPart? part = null)
        {
            var items = new List<Item>();

            for (var i = 0; i < amount; i++) items.Add(GetItem(part));

            return items;
        }

        private static Sprite GetIcon(string iconName)
        {
            if (_icons == null) LoadIcons();

            return _icons.GetSprite(iconName);
        }

        private static void LoadIcons()
        {
            _icons = Resources.Load<SpriteCollection>("Sprites/Icons");
        }
    }
}