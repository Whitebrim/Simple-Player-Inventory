using UnityEngine.UIElements;

namespace Game.UI
{
    public class InventoryView
    {
        private readonly InventoryViewModel _viewModel;
        private readonly VisualElement _root;
        private readonly VisualElement _panel;
        private readonly VisualElement _gridContainer;
        private readonly VisualElement[] _slotElements;
        private readonly VisualElement[] _slotIcons;
        private readonly Label[] _slotAmounts;

        public InventoryView(UIDocument document, InventoryViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = document.rootVisualElement;
            _panel = _root.Q("inventory-panel");
            _gridContainer = _root.Q("grid-container");

            int slotCount = viewModel.SlotCount;
            _slotElements = new VisualElement[slotCount];
            _slotIcons = new VisualElement[slotCount];
            _slotAmounts = new Label[slotCount];

            BuildSlots(slotCount);
            SetupDragDrop();
            SetupRightClick(slotCount);

            _viewModel.SlotUpdated += UpdateSlot;
            _viewModel.VisibilityChanged += OnVisibilityChanged;

            SetVisible(false);
        }

        private void BuildSlots(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = new VisualElement();
                slot.AddToClassList("slot");

                var icon = new VisualElement();
                icon.AddToClassList("slot-icon");
                slot.Add(icon);

                var amount = new Label();
                amount.AddToClassList("slot-amount");
                amount.pickingMode = PickingMode.Ignore;
                slot.Add(amount);

                _gridContainer.Add(slot);
                _slotElements[i] = slot;
                _slotIcons[i] = icon;
                _slotAmounts[i] = amount;

                UpdateSlot(i, _viewModel.GetSlotData(i));
            }
        }

        private void SetupDragDrop()
        {
            var manipulator = new DragDropManipulator(_slotElements, _root, CreateGhostElement);
            _gridContainer.AddManipulator(manipulator);

            manipulator.Moved += (from, to) => _viewModel.RequestMove(from, to);
            manipulator.DroppedOutside += from => _viewModel.RequestDrop(from);
        }

        private void SetupRightClick(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = i;
                _slotElements[i].RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button == 1)
                    {
                        _viewModel.RequestDrop(index);
                        evt.StopPropagation();
                    }
                });
            }
        }

        private void UpdateSlot(int index, SlotViewData data)
        {
            if (data.IsEmpty)
            {
                _slotIcons[index].style.display = DisplayStyle.None;
                _slotAmounts[index].style.display = DisplayStyle.None;
                return;
            }

            _slotIcons[index].style.display = DisplayStyle.Flex;
            _slotIcons[index].style.backgroundColor = data.Color;

            if (data.ShowAmount)
            {
                _slotAmounts[index].text = data.Amount.ToString();
                _slotAmounts[index].style.display = DisplayStyle.Flex;
            }
            else
            {
                _slotAmounts[index].style.display = DisplayStyle.None;
            }
        }

        private void OnVisibilityChanged(bool isOpen)
        {
            SetVisible(isOpen);
        }

        private void SetVisible(bool visible)
        {
            _panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private VisualElement CreateGhostElement(int slotIndex)
        {
            var data = _viewModel.GetSlotData(slotIndex);

            if (data.IsEmpty)
                return null;

            var ghost = new VisualElement();
            ghost.style.width = 60;
            ghost.style.height = 60;
            ghost.style.backgroundColor = data.Color;
            ghost.style.opacity = 0.7f;
            ghost.style.borderTopLeftRadius = 6;
            ghost.style.borderTopRightRadius = 6;
            ghost.style.borderBottomLeftRadius = 6;
            ghost.style.borderBottomRightRadius = 6;

            return ghost;
        }
    }
}
