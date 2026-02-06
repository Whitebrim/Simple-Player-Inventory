using System;
using Game.Data;

namespace Game.Services
{
    public interface IInventoryService
    {
        int SlotCount { get; }
        event Action<int> SlotChanged;

        int TryAdd(ItemData data);
        MoveResult MoveSlot(int fromIndex, int toIndex, int amount = -1);
        ItemData RemoveFromSlot(int slotIndex, int amount = -1);
        InventorySlot GetSlot(int index);
    }
}
