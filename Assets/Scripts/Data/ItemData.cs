using System;

namespace Game.Data
{
    [Serializable]
    public struct ItemData
    {
        public string ItemId;
        public int Amount;

        public bool IsEmpty => string.IsNullOrEmpty(ItemId) || Amount <= 0;

        public ItemData(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
