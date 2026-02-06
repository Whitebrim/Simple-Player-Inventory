using UnityEngine;

namespace Game.UI
{
    public readonly struct SlotViewData
    {
        public bool IsEmpty => Name == null;
        public Color Color { get; }
        public string Name { get; }
        public int Amount { get; }
        public bool ShowAmount { get; }

        public SlotViewData(Color color, string name, int amount, bool showAmount)
        {
            Color = color;
            Name = name;
            Amount = amount;
            ShowAmount = showAmount;
        }
    }
}
