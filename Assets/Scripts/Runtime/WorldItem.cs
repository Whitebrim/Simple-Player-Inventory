using Game.Data;
using Game.Services;
using UnityEngine;

namespace Game.Runtime
{
    public class WorldItem : MonoBehaviour
    {
        [SerializeField] private string _itemId;
        [SerializeField] private int _amount = 1;

        public ItemData Data => new(_itemId, _amount);

        public void Init(IItemDatabase database)
        {
            ApplyVisual(database);
        }

        public void Init(IItemDatabase database, ItemData data)
        {
            _itemId = data.ItemId;
            _amount = data.Amount;
            ApplyVisual(database);
        }

        public void ReduceAmount(int count)
        {
            _amount = Mathf.Max(0, _amount - count);
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
