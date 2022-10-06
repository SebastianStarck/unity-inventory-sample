using System.Collections.Generic;
using System.Linq;
using Generic;
using Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Inventory
{
    public enum MouseInteraction
    {
        RightClick,
        LeftClick,
        MiddleClick
    }

    /// <summary>
    /// Designated partial for UI related events and rendering
    /// </summary>
    public partial class InventoryController : MonoBehaviour
    {
        private const string GhostIconClass = "GhostIcon";
        private InventoryController _inventoryController;

        private List<InventorySlot> _inventorySlots;
        private readonly Dictionary<EquipmentPart, InventorySlot> _equipmentSlots = new();

        private VisualElement _root;
        private VisualElement _equipmentContainer;
        private VisualElement _inventoryContainer;

        private static VisualElement _ghostIcon;
        private static bool _isDragging;
        private static InventorySlot _originalSlot;

        #region Init
        /// <summary>
        /// Bind visual containers and instantiate data holders. 
        /// </summary>
        private void InitUI()
        {
            _inventorySlots = new List<InventorySlot>(inventory.size);
            _inventoryController = FindObjectOfType<InventoryController>();

            _root = GetComponent<UIDocument>().rootVisualElement;
            _ghostIcon = _root.Query(GhostIconClass);

            InstantiateEquipmentSlots();
            InstantiateInventorySlots();
            InstantiateGhostIcon();
        }

        /// <summary>
        /// Bind inventory's slot container & instantiate InventorySlot collection
        /// </summary>
        private void InstantiateInventorySlots()
        {
            _inventoryContainer = _root.Query("InventoryContainer");

            for (var i = 0; i < 18; i++)
            {
                InventorySlot inventorySlot = new InventorySlot { SlotPosition = i };
                // See Inventory.InventorySlot.OnPointerDown InventorySlot.cs:39     
                // ↓ UI event binding ↓
                inventorySlot.SlotInteraction += OnSlotInteraction;
                _inventorySlots.Add(inventorySlot);
                _inventoryContainer.Add(inventorySlot);
            }
        }

        private void InstantiateEquipmentSlots()
        {
            _equipmentContainer = _root.Query("EquipmentContainer");

            foreach (var part in Enum<EquipmentPart>.Values)
            {
                InventorySlot equipmentSlot = new InventorySlot(part.ToString()) { Part = part, };
                // See Inventory.InventorySlot.OnPointerDown InventorySlot.cs:39     
                // ↓ UI event binding ↓
                equipmentSlot.SlotInteraction += OnSlotInteraction;
                _equipmentSlots.Add(part, equipmentSlot);
                _equipmentContainer.Add(equipmentSlot);
            }
        }

        /// <summary>
        /// Register required events for drag system
        /// </summary>
        private void InstantiateGhostIcon()
        {
            _ghostIcon.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _ghostIcon.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }
        #endregion

        /// <summary>
        /// Force update of inventory icons display
        /// </summary>
        private void UIUpdateInventorySlots()
        {
            for (var i = 0; i < inventory.Items.Length; i++)
            {
                var item = inventory.Items[i];
                var slot = _inventorySlots[i];
                slot.HoldItem(item);
            }
        }

        /// <summary>
        /// Force update of equipment icons display
        /// </summary>
        private void UIUpdateEquipmentSlots()
        {
            foreach (var (part, item) in _playerEquipment)
            {
                InventorySlot equipmentSlot = _equipmentSlots[part];
                equipmentSlot.HoldItem(item);
            }
        }

        /// <summary>
        /// Event handler for Inventory Slot's mouse interaction
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="evt"></param>
        private void OnSlotInteraction(InventorySlot slot, PointerDownEvent evt)
        {
            switch (evt.button)
            {
                case (int)MouseInteraction.RightClick:
                    StartDrag(evt.position, slot);
                    return;

                case (int)MouseInteraction.LeftClick:
                    if (slot.Part != null) UnequipItemInSlot(slot);
                    else EquipItemInSlot(slot);

                    return;

                case (int)MouseInteraction.MiddleClick:
                    // Do ???
                    return;
            }
        }

        #region Drag
        /// <summary>
        /// Initiate the drag by enabling the ghost icon
        /// </summary>
        /// <param name="position">Mouse Position</param>
        /// <param name="slot">Inventory Slot that the player has selected</param>
        private static void StartDrag(Vector2 position, InventorySlot slot)
        {
            if (slot.Item == null) return;

            slot.Icon.image = null;
            _isDragging = true;
            _originalSlot = slot;

            // Center the ghost icon based on the style size
            _ghostIcon.style.top = position.y - _ghostIcon.layout.height / 2;
            _ghostIcon.style.left = position.x - _ghostIcon.layout.width / 2;

            _ghostIcon.style.backgroundImage = GetItemByGuid(slot.ItemGuid).Icon.texture;
            // This is the cheap trick used for recoloring icons. Not really needed
            _ghostIcon.style.unityBackgroundImageTintColor = slot.Item.Color;
            _ghostIcon.style.visibility = Visibility.Visible;
        }

        /// <summary>
        /// Perform the drag while the mouse pointer is above the ghost icon, which is... always
        /// </summary>
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;

            _ghostIcon.style.top = evt.position.y - _ghostIcon.layout.height / 2;
            _ghostIcon.style.left = evt.position.x - _ghostIcon.layout.width / 2;
        }

        /// <summary>
        /// Ends drags on mouse click release. 
        /// </summary>
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;

            var closestInventorySlot = GetTouchedInventorySlots().FirstOrDefault();
            if (closestInventorySlot is not null)
            {
                SwapItemToSlot(closestInventorySlot);

                CleanupDrag();
                return;
            }

            // FIXME: Handle all slots together and check if its equipment on the fly?
            var closestEquipmentSlot = GetTouchedEquipmentSlots().FirstOrDefault();
            if (closestEquipmentSlot is not null)
            {
                var draggedItem = _originalSlot.Item;
                var draggedItemPart = draggedItem.Part ?? default;

                if (draggedItemPart == closestEquipmentSlot.Part && closestEquipmentSlot != _originalSlot)
                {
                    EquipItemInSlot(_originalSlot);

                    CleanupDrag();
                    return;
                }
            }

            _originalSlot.Icon.image = GetItemByGuid(_originalSlot.ItemGuid).Icon.texture;
            CleanupDrag();
        }

        /// <summary>
        /// Returns all the touched inventory slots by the ghost icon 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<InventorySlot> GetTouchedEquipmentSlots()
        {
            return _equipmentSlots
                .Values
                .Where(x => x.worldBound.Overlaps(_ghostIcon.worldBound))
                .OrderBy(x => Vector2.Distance(x.worldBound.position, _ghostIcon.worldBound.position));
        }

        /// <summary>
        /// Returns all the touched inventory slots by the ghost icon 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<InventorySlot> GetTouchedInventorySlots()
        {
            return _inventorySlots
                .Where(x => x.worldBound.Overlaps(_ghostIcon.worldBound))
                .OrderBy(x => Vector2.Distance(x.worldBound.position, _ghostIcon.worldBound.position));
        }

        /// <summary>
        /// Swaps the items between the original dragged inventory slot and another.
        /// </summary>
        /// <param name="closestSlot"></param>
        private void SwapItemToSlot(InventorySlot closestSlot)
        {
            var isValidTarget = closestSlot != null && closestSlot != _originalSlot;

            if (isValidTarget) _inventoryController.SwapItemsInSlots(_originalSlot, closestSlot);
            else _originalSlot.RestoreItemIcon();
        }

        private static void CleanupDrag()
        {
            _isDragging = false;
            _originalSlot = null;
            _ghostIcon.style.visibility = Visibility.Hidden;
        }
        #endregion
    }
}