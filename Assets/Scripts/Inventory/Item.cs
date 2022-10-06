using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Inventory
{
    [Serializable]
    public class Item
    {
        public string Name;
        public string GUID;
        public Sprite Icon;
        public Color Color = Random.ColorHSV();

        public EquipmentPart? Part;
        public int InventoryPosition;

        public override string ToString() => $"{Name}-{GUID}";
    }
}