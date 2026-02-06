using UnityEngine;

namespace Game.UI
{
    public readonly struct SlotViewData
    {
        public bool IsEmpty => Name == null;
        public Color Color { get; }
        public string Name { get; }
        public int Amount { get; }
        public int MaxStack { get; }
        public bool Stackable { get; }
        public bool ShowAmount { get; }

        public SlotViewData(Color color, string name, int amount, int maxStack, bool stackable, bool showAmount)
        {
            Color = color;
            Name = name;
            Amount = amount;
            MaxStack = maxStack;
            Stackable = stackable;
            ShowAmount = showAmount;
        }
    }
}
