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

        public bool TryAdd(string itemId, int amount = 1)
        {
            var definition = _database.GetById(itemId);
            if (definition == null)
                return false;

            if (!HasSpaceFor(itemId, amount, definition))
                return false;

            int remaining = amount;

            if (definition.Stackable)
            {
                for (int i = 0; i < _slots.Length && remaining > 0; i++)
                {
                    if (_slots[i].ItemId != itemId)
                        continue;

                    int space = definition.MaxStack - _slots[i].Amount;
                    int toAdd = Mathf.Min(remaining, space);

                    if (toAdd <= 0)
                        continue;

                    _slots[i].Amount += toAdd;
                    remaining -= toAdd;
                    SlotChanged?.Invoke(i);
                }
            }

            while (remaining > 0)
            {
                int emptyIndex = FindEmptySlot();
                int toPlace = definition.Stackable
                    ? Mathf.Min(remaining, definition.MaxStack)
                    : 1;

                _slots[emptyIndex].Set(itemId, toPlace);
                remaining -= toPlace;
                SlotChanged?.Invoke(emptyIndex);
            }

            return true;
        }

        public MoveResult MoveSlot(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return new MoveResult(true, 0);

            var from = _slots[fromIndex];
            var to = _slots[toIndex];

            if (from.IsEmpty)
                return new MoveResult(false, 0);

            if (to.IsEmpty)
            {
                to.Set(from.ItemId, from.Amount);
                from.Clear();
                NotifySlots(fromIndex, toIndex);
                return new MoveResult(true, 0);
            }

            if (from.ItemId == to.ItemId)
            {
                var definition = _database.GetById(from.ItemId);

                if (definition.Stackable)
                {
                    int space = definition.MaxStack - to.Amount;
                    int transfer = Mathf.Min(from.Amount, space);

                    to.Amount += transfer;
                    from.Amount -= transfer;

                    if (from.Amount <= 0)
                        from.Clear();

                    NotifySlots(fromIndex, toIndex);
                    return new MoveResult(true, from.IsEmpty ? 0 : from.Amount);
                }
            }

            string tempId = from.ItemId;
            int tempAmount = from.Amount;
            from.Set(to.ItemId, to.Amount);
            to.Set(tempId, tempAmount);
            NotifySlots(fromIndex, toIndex);
            return new MoveResult(true, 0);
        }

        public DropData RemoveFromSlot(int slotIndex, int amount = -1)
        {
            var slot = _slots[slotIndex];

            if (slot.IsEmpty)
                return new DropData(null, 0);

            int toRemove = amount < 0 ? slot.Amount : Mathf.Min(amount, slot.Amount);
            string itemId = slot.ItemId;

            slot.Amount -= toRemove;

            if (slot.Amount <= 0)
                slot.Clear();

            SlotChanged?.Invoke(slotIndex);
            return new DropData(itemId, toRemove);
        }

        private bool HasSpaceFor(string itemId, int amount, ItemDefinition definition)
        {
            int available = 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    available += definition.Stackable ? definition.MaxStack : 1;
                }
                else if (definition.Stackable && _slots[i].ItemId == itemId)
                {
                    available += definition.MaxStack - _slots[i].Amount;
                }

                if (available >= amount)
                    return true;
            }

            return false;
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
