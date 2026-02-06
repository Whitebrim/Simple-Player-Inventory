using System;
using Game.Data;
using Game.Services;
using UnityEngine;

namespace Game.UI
{
    public class InventoryViewModel
    {
        private readonly IInventoryService _inventory;
        private readonly IItemDatabase _database;
        private readonly InputService _input;
        private int _selectedSlotIndex = -1;

        public int SlotCount => _inventory.SlotCount;
        public bool IsOpen { get; private set; }

        public event Action<int, SlotViewData> SlotUpdated;
        public event Action<bool> VisibilityChanged;
        public event Action<ItemData> DropRequested;
        public event Action<SlotViewData> DetailShown;
        public event Action DetailHidden;

        public InventoryViewModel(IInventoryService inventory, IItemDatabase database, InputService input)
        {
            _inventory = inventory;
            _database = database;
            _input = input;

            _inventory.SlotChanged += OnSlotChanged;
            _input.InventoryTogglePressed += ToggleVisibility;
            _input.InventoryClosePressed += CloseInventory;
        }

        public SlotViewData GetSlotData(int index)
        {
            var slot = _inventory.GetSlot(index);

            if (slot.IsEmpty)
                return default;

            var definition = _database.GetById(slot.ItemId);

            return new SlotViewData(
                definition.Color,
                definition.Name,
                slot.Amount,
                definition.MaxStack,
                definition.Stackable,
                definition.Stackable && slot.Amount > 1
            );
        }

        public int GetMoveAmount(int slotIndex, DragModifier modifier)
        {
            var slot = _inventory.GetSlot(slotIndex);

            if (slot.IsEmpty)
                return 0;

            return modifier switch
            {
                DragModifier.HalfStack => Mathf.Max(1, slot.Amount / 2),
                DragModifier.SingleItem => 1,
                _ => slot.Amount
            };
        }

        public void RequestMove(int fromIndex, int toIndex, int amount)
        {
            _inventory.MoveSlot(fromIndex, toIndex, amount);
        }

        public void RequestDrop(int slotIndex, int amount)
        {
            var data = _inventory.RemoveFromSlot(slotIndex, amount);

            if (!data.IsEmpty)
                DropRequested?.Invoke(data);
        }

        public void RequestSort()
        {
            DeselectSlot();
            _inventory.Sort();
        }

        public void SelectSlot(int index)
        {
            if (index == _selectedSlotIndex)
            {
                DeselectSlot();
                return;
            }

            var slot = _inventory.GetSlot(index);

            if (slot.IsEmpty)
            {
                DeselectSlot();
                return;
            }

            _selectedSlotIndex = index;
            DetailShown?.Invoke(GetSlotData(index));
        }

        public void DeselectSlot()
        {
            _selectedSlotIndex = -1;
            DetailHidden?.Invoke();
        }

        private void ToggleVisibility()
        {
            SetOpen(!IsOpen);
        }

        private void CloseInventory()
        {
            if (IsOpen)
                SetOpen(false);
        }

        private void SetOpen(bool open)
        {
            IsOpen = open;
            _input.SetGameplayActive(!open);

            if (!open)
                DeselectSlot();

            VisibilityChanged?.Invoke(open);
        }

        private void OnSlotChanged(int index)
        {
            SlotUpdated?.Invoke(index, GetSlotData(index));

            if (index != _selectedSlotIndex)
                return;

            var slot = _inventory.GetSlot(index);

            if (slot.IsEmpty)
                DeselectSlot();
            else
                DetailShown?.Invoke(GetSlotData(index));
        }
    }
}
