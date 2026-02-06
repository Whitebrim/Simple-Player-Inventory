using Game.Services;
using UnityEngine;

namespace Game.Runtime
{
    public class ItemDropper
    {
        private readonly IItemDatabase _database;
        private readonly WorldItem _prefab;

        public ItemDropper(IItemDatabase database, WorldItem prefab)
        {
            _database = database;
            _prefab = prefab;
        }

        public void Drop(string itemId, int amount, Vector3 position)
        {
            var instance = Object.Instantiate(_prefab, position, Quaternion.identity);
            instance.Init(_database, itemId, amount);
        }
    }
}
