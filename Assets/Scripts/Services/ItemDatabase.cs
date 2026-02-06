using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Services
{
    public class ItemDatabase : IItemDatabase
    {
        private readonly Dictionary<string, ItemDefinition> _items = new();
        private readonly List<ItemDefinition> _allItems = new();

        public IReadOnlyList<ItemDefinition> All => _allItems;

        public ItemDatabase(string json)
        {
            var raw = JsonUtility.FromJson<ItemCollectionRaw>(json);

            foreach (var entry in raw.items)
            {
                ColorUtility.TryParseHtmlString(entry.color, out var color);

                var definition = new ItemDefinition(
                    entry.id,
                    entry.name,
                    color,
                    entry.stackable,
                    entry.maxStack
                );

                _items[definition.Id] = definition;
                _allItems.Add(definition);
            }
        }

        public ItemDefinition GetById(string id)
        {
            return _items.GetValueOrDefault(id);
        }
    }
}
