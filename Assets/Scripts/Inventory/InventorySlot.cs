using Code.Inventory;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Inventory
{
    public delegate void SlotInteraction(InventorySlot slot, PointerDownEvent ev);

    // FIXME: Create a variant for equipment slots
    public class InventorySlot : VisualElement
    {
        public Image Icon;
        public int SlotPosition;
        public string ItemGuid = "";
        public Item Item;
        public EquipmentPart? Part;
        public bool IsEquipmentSlot => Part is not null;

        /// <summary>
        /// Event used for emitting slot interactions
        /// This bubbles up to InventoryController.UI
        /// See InventoryController.UI.cs:116
        /// </summary>
        public event SlotInteraction SlotInteraction;

        public InventorySlot() : this(null)
        {
        }

        public InventorySlot(string className = null)
        {
            Icon = new Image { visible = false };
            Add(Icon);

            Icon.AddToClassList("slotIcon");
            AddToClassList("slotContainer");
            if (className is not null) AddToClassList(className);

            // Register mouse click event using VisualElements callback handler
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        private void OnPointerDown(PointerDownEvent evt) => SlotInteraction?.Invoke(this, evt);

        /// <summary>
        /// Sets the Item, Icon, and GUID properties
        /// </summary>
        /// <param name="item"></param>
        public void HoldItem([CanBeNull] Item item)
        {
            Icon.image = item?.Icon.texture;
            Icon.tintColor = item?.Color ?? default;
            ItemGuid = item?.GUID;
            Item = item;
            Icon.style.visibility = item is not null ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Clears the Icon and GUID properties
        /// </summary>
        public void DropItem()
        {
            ItemGuid = "";
            Icon.image = null;
            Item = null;
        }

        public override string ToString() => $"Inventory slot {SlotPosition}";

        // Some UXML magic things I guess

        #region UXML
        [Preserve]
        public new class UxmlFactory : UxmlFactory<InventorySlot, UxmlTraits>
        {
        }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
        #endregion

        public void RestoreItemIcon()
        {
            HoldItem(Item);
        }
    }
}