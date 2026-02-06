using System.Collections.Generic;
using Game.Services;
using Game.UI;
using UnityEngine;

namespace Game.Runtime
{
    public class ItemPickupHandler : MonoBehaviour
    {
        private InputService _input;
        private IInventoryService _inventory;
        private InteractionHintUI _hint;
        private NotificationUI _notification;
        private readonly List<WorldItem> _itemsInRange = new();
        private WorldItem _closestItem;

        public void Init(
            InputService input,
            IInventoryService inventory,
            InteractionHintUI hint,
            NotificationUI notification)
        {
            _input = input;
            _inventory = inventory;
            _hint = hint;
            _notification = notification;

            _input.InteractPressed += OnInteract;
        }

        private void OnDestroy()
        {
            if (_input != null)
                _input.InteractPressed -= OnInteract;
        }

        private void Update()
        {
            UpdateClosestItem();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<WorldItem>(out var item))
                _itemsInRange.Add(item);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<WorldItem>(out var item))
                _itemsInRange.Remove(item);
        }

        private void OnInteract()
        {
            if (_closestItem == null)
                return;

            int added = _inventory.TryAdd(_closestItem.Data);

            if (added <= 0)
            {
                _notification.Show("Inventory is full");
                return;
            }

            if (added >= _closestItem.Data.Amount)
            {
                var picked = _closestItem;
                _itemsInRange.Remove(picked);
                _closestItem = null;
                _hint.Hide();
                Destroy(picked.gameObject);
            }
            else
            {
                _closestItem.ReduceAmount(added);
            }
        }

        private void UpdateClosestItem()
        {
            _itemsInRange.RemoveAll(item => item == null);

            WorldItem nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var item in _itemsInRange)
            {
                float distance = Vector3.Distance(transform.position, item.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = item;
                }
            }

            if (nearest != _closestItem)
            {
                _closestItem = nearest;

                if (_closestItem != null)
                    _hint.Show("Press E");
                else
                    _hint.Hide();
            }
        }
    }
}
