using System.IO;
using Game.Runtime;
using Game.Services;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Infrastructure
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private ItemPickupHandler _itemPickupHandler;
        [SerializeField] private UIDocument _inventoryDocument;
        [SerializeField] private UIDocument _hintDocument;
        [SerializeField] private UIDocument _notificationDocument;
        [SerializeField] private WorldItem _worldItemPrefab;

        private InputService _inputService;

        private void Awake()
        {
            var container = new DIContainer();

            _inputService = new InputService();
            container.Register(_inputService);

            string jsonPath = Path.Combine(Application.streamingAssetsPath, "items.json");
            string json = File.ReadAllText(jsonPath);
            var itemDatabase = new ItemDatabase(json);
            container.Register<IItemDatabase>(itemDatabase);

            var inventoryService = new InventoryService(itemDatabase);
            container.Register<IInventoryService>(inventoryService);

            var itemDropper = new ItemDropper(itemDatabase, _worldItemPrefab, _playerController.transform);

            _playerController.Init(_inputService);

            var hintUI = new InteractionHintUI(_hintDocument);
            var notificationUI = new NotificationUI(_notificationDocument);
            _itemPickupHandler.Init(_inputService, inventoryService, hintUI, notificationUI);

            var viewModel = new InventoryViewModel(inventoryService, itemDatabase, _inputService);
            var inventoryView = new InventoryView(_inventoryDocument, viewModel);

            viewModel.DropRequested += data => itemDropper.Drop(data);

            foreach (var worldItem in FindObjectsByType<WorldItem>(FindObjectsSortMode.None))
                worldItem.Init(itemDatabase);

            _inputService.SetGameplayActive(true);
        }

        private void OnDestroy()
        {
            _inputService?.Dispose();
        }
    }
}
