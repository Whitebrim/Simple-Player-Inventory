using Game.Data;
using Game.Services;
using UnityEngine;

namespace Game.Runtime
{
    public class ItemDropper
    {
        private const float DefaultDropOffset = 2f;

        private readonly IItemDatabase _database;
        private readonly WorldItem _prefab;
        private readonly Transform _playerTransform;

        public ItemDropper(IItemDatabase database, WorldItem prefab, Transform playerTransform)
        {
            _database = database;
            _prefab = prefab;
            _playerTransform = playerTransform;
        }

        public void Drop(ItemData data)
        {
            Vector3 position = _playerTransform.position
                               + _playerTransform.forward * DefaultDropOffset;

            var instance = Object.Instantiate(_prefab, position, Quaternion.identity);
            instance.Init(_database, data);
        }
    }
}
