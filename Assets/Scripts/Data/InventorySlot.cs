namespace Game.Data
{
    public class InventorySlot
    {
        public string ItemId { get; private set; }
        public int Amount { get; set; }
        public bool IsEmpty => ItemId == null;

        public void Set(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public void Clear()
        {
            ItemId = null;
            Amount = 0;
        }
    }
}
