namespace Game.Data
{
    public class InventorySlot
    {
        public string ItemId { get; private set; }
        public int Amount { get; set; }
        public bool IsEmpty => ItemId == null;

        public ItemData Data => new(ItemId, Amount);

        public void Set(ItemData data)
        {
            ItemId = data.ItemId;
            Amount = data.Amount;
        }

        public void Clear()
        {
            ItemId = null;
            Amount = 0;
        }
    }
}
