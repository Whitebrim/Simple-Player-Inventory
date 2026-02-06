namespace Game.Data
{
    public readonly struct DropData
    {
        public string ItemId { get; }
        public int Amount { get; }
        public bool IsEmpty => ItemId == null;

        public DropData(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
