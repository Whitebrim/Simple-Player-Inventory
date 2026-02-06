using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Services
{
    public class InputService : IDisposable
    {
        private readonly InputActionMap _gameplayMap;
        private readonly InputActionMap _uiMap;
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _interactAction;
        private readonly InputAction _inventoryToggleAction;
        private readonly InputAction _inventoryCloseAction;

        public Vector2 MoveDirection => _moveAction.ReadValue<Vector2>();
        public Vector2 LookDelta => _lookAction.ReadValue<Vector2>();

        public event Action InteractPressed;
        public event Action InventoryTogglePressed;
        public event Action InventoryClosePressed;

        public InputService()
        {
            _gameplayMap = new InputActionMap("Gameplay");

            _moveAction = _gameplayMap.AddAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _lookAction = _gameplayMap.AddAction("Look", InputActionType.Value);
            _lookAction.AddBinding("<Mouse>/delta");

            _interactAction = _gameplayMap.AddAction("Interact", InputActionType.Button);
            _interactAction.AddBinding("<Keyboard>/e");

            _uiMap = new InputActionMap("UI");

            _inventoryToggleAction = _uiMap.AddAction("InventoryToggle", InputActionType.Button);
            _inventoryToggleAction.AddBinding("<Keyboard>/tab");

            _inventoryCloseAction = _uiMap.AddAction("InventoryClose", InputActionType.Button);
            _inventoryCloseAction.AddBinding("<Keyboard>/escape");

            _interactAction.performed += OnInteract;
            _inventoryToggleAction.performed += OnInventoryToggle;
            _inventoryCloseAction.performed += OnInventoryClose;

            _uiMap.Enable();
        }

        public void SetGameplayActive(bool active)
        {
            if (active)
            {
                _gameplayMap.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                _gameplayMap.Disable();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Dispose()
        {
            _interactAction.performed -= OnInteract;
            _inventoryToggleAction.performed -= OnInventoryToggle;
            _inventoryCloseAction.performed -= OnInventoryClose;

            _gameplayMap.Disable();
            _uiMap.Disable();
            _gameplayMap.Dispose();
            _uiMap.Dispose();
        }

        private void OnInteract(InputAction.CallbackContext ctx) => InteractPressed?.Invoke();
        private void OnInventoryToggle(InputAction.CallbackContext ctx) => InventoryTogglePressed?.Invoke();
        private void OnInventoryClose(InputAction.CallbackContext ctx) => InventoryClosePressed?.Invoke();
    }
}
