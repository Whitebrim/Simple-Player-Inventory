using System;
using Game.Data;

namespace Game.Services
{
    public interface IInventoryService
    {
        int SlotCount { get; }
        event Action<int> SlotChanged;

        bool TryAdd(string itemId, int amount = 1);
        MoveResult MoveSlot(int fromIndex, int toIndex);
        DropData RemoveFromSlot(int slotIndex, int amount = -1);
        InventorySlot GetSlot(int index);
    }
}
