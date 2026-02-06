using System;
using Game.Services;

namespace Game.UI
{
    public class InventoryViewModel
    {
        private readonly IInventoryService _inventory;
        private readonly IItemDatabase _database;
        private readonly InputService _input;

        public int SlotCount => _inventory.SlotCount;
        public bool IsOpen { get; private set; }

        public event Action<int, SlotViewData> SlotUpdated;
        public event Action<bool> VisibilityChanged;
        public event Action<string, int> DropRequested;

        public InventoryViewModel(IInventoryService inventory, IItemDatabase database, InputService input)
        {
            _inventory = inventory;
            _database = database;
            _input = input;

            _inventory.SlotChanged += OnSlotChanged;
            _input.InventoryTogglePressed += ToggleVisibility;
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
                definition.Stackable && slot.Amount > 1
            );
        }

        public void RequestMove(int fromIndex, int toIndex)
        {
            _inventory.MoveSlot(fromIndex, toIndex);
        }

        public void RequestDrop(int slotIndex)
        {
            var data = _inventory.RemoveFromSlot(slotIndex);

            if (!data.IsEmpty)
                DropRequested?.Invoke(data.ItemId, data.Amount);
        }

        private void ToggleVisibility()
        {
            IsOpen = !IsOpen;
            _input.SetGameplayActive(!IsOpen);
            VisibilityChanged?.Invoke(IsOpen);
        }

        private void OnSlotChanged(int index)
        {
            SlotUpdated?.Invoke(index, GetSlotData(index));
        }
    }
}
