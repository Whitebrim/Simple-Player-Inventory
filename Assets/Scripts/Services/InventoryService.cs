using System;
using Game.Data;
using UnityEngine;

namespace Game.Services
{
    public class InventoryService : IInventoryService
    {
        private const int DefaultSlotCount = 12;

        private readonly InventorySlot[] _slots;
        private readonly IItemDatabase _database;

        public int SlotCount => _slots.Length;
        public event Action<int> SlotChanged;

        public InventoryService(IItemDatabase database)
        {
            _database = database;
            _slots = new InventorySlot[DefaultSlotCount];

            for (int i = 0; i < _slots.Length; i++)
                _slots[i] = new InventorySlot();
        }

        public InventorySlot GetSlot(int index) => _slots[index];

        public int TryAdd(ItemData data)
        {
            var definition = _database.GetById(data.ItemId);
            if (definition == null)
                return 0;

            int remaining = data.Amount;
            int added = 0;

            if (definition.Stackable)
            {
                for (int i = 0; i < _slots.Length && remaining > 0; i++)
                {
                    if (_slots[i].ItemId != data.ItemId)
                        continue;

                    int space = definition.MaxStack - _slots[i].Amount;
                    int toAdd = Mathf.Min(remaining, space);

                    if (toAdd <= 0)
                        continue;

                    _slots[i].Amount += toAdd;
                    remaining -= toAdd;
                    added += toAdd;
                    SlotChanged?.Invoke(i);
                }
            }

            while (remaining > 0)
            {
                int emptyIndex = FindEmptySlot();
                if (emptyIndex < 0)
                    break;

                int toPlace = definition.Stackable
                    ? Mathf.Min(remaining, definition.MaxStack)
                    : 1;

                _slots[emptyIndex].Set(new ItemData(data.ItemId, toPlace));
                remaining -= toPlace;
                added += toPlace;
                SlotChanged?.Invoke(emptyIndex);
            }

            return added;
        }

        public MoveResult MoveSlot(int fromIndex, int toIndex, int amount = -1)
        {
            if (fromIndex == toIndex)
                return new MoveResult(true, 0);

            var from = _slots[fromIndex];
            var to = _slots[toIndex];

            if (from.IsEmpty)
                return new MoveResult(false, 0);

            int moveAmount = amount < 0 ? from.Amount : Mathf.Min(amount, from.Amount);

            if (to.IsEmpty)
            {
                to.Set(new ItemData(from.ItemId, moveAmount));
                from.Amount -= moveAmount;

                if (from.Amount <= 0)
                    from.Clear();

                NotifySlots(fromIndex, toIndex);
                return new MoveResult(true, from.IsEmpty ? 0 : from.Amount);
            }

            if (from.ItemId == to.ItemId)
            {
                var definition = _database.GetById(from.ItemId);

                if (definition.Stackable)
                {
                    int space = definition.MaxStack - to.Amount;
                    int transfer = Mathf.Min(moveAmount, space);

                    to.Amount += transfer;
                    from.Amount -= transfer;

                    if (from.Amount <= 0)
                        from.Clear();

                    NotifySlots(fromIndex, toIndex);
                    return new MoveResult(true, from.IsEmpty ? 0 : from.Amount);
                }
            }

            if (moveAmount < from.Amount)
                return new MoveResult(false, from.Amount);

            var temp = from.Data;
            from.Set(to.Data);
            to.Set(temp);
            NotifySlots(fromIndex, toIndex);
            return new MoveResult(true, 0);
        }

        public ItemData RemoveFromSlot(int slotIndex, int amount = -1)
        {
            var slot = _slots[slotIndex];

            if (slot.IsEmpty)
                return default;

            int toRemove = amount < 0 ? slot.Amount : Mathf.Min(amount, slot.Amount);
            var removed = new ItemData(slot.ItemId, toRemove);

            slot.Amount -= toRemove;

            if (slot.Amount <= 0)
                slot.Clear();

            SlotChanged?.Invoke(slotIndex);
            return removed;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty)
                    return i;
            }

            return -1;
        }

        private void NotifySlots(int a, int b)
        {
            SlotChanged?.Invoke(a);
            SlotChanged?.Invoke(b);
        }
    }
}
