using System;

namespace Game.Data
{
    [Serializable]
    public class ItemRawData
    {
        public string id;
        public string name;
        public string color;
        public bool stackable;
        public int maxStack;
    }

    [Serializable]
    public class ItemCollectionRaw
    {
        public ItemRawData[] items;
    }
}
