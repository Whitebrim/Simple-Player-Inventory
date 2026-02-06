using UnityEngine;

namespace Game.Data
{
    public class ItemDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public Color Color { get; }
        public bool Stackable { get; }
        public int MaxStack { get; }

        public ItemDefinition(string id, string name, Color color, bool stackable, int maxStack)
        {
            Id = id;
            Name = name;
            Color = color;
            Stackable = stackable;
            MaxStack = maxStack;
        }
    }
}
