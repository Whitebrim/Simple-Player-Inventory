using Game.Services;
using UnityEngine;

namespace Game.Runtime
{
    public class WorldItem : MonoBehaviour
    {
        [SerializeField] private string _itemId;

        public string ItemId => _itemId;
        public int Amount { get; private set; } = 1;

        public void Init(IItemDatabase database)
        {
            Amount = 1;
            ApplyVisual(database);
        }

        public void Init(IItemDatabase database, string itemId, int amount)
        {
            _itemId = itemId;
            Amount = amount;
            ApplyVisual(database);
        }

        private void ApplyVisual(IItemDatabase database)
        {
            var definition = database.GetById(_itemId);

            if (definition == null)
                return;

            var meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
                meshRenderer.material.color = definition.Color;
        }
    }
}
