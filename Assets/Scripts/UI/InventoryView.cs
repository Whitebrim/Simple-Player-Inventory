using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class InventoryView
    {
        private const int GhostSize = 60;
        private const float GhostOpacity = 0.7f;
        private const int GhostBorderRadius = 6;
        private const int GhostLabelFontSize = 14;

        private readonly InventoryViewModel _viewModel;
        private readonly VisualElement _root;
        private readonly VisualElement _layout;
        private readonly VisualElement _panel;
        private readonly VisualElement _gridContainer;
        private readonly VisualElement _detailPanel;
        private readonly Label _detailName;
        private readonly Label _detailAmount;
        private readonly VisualElement[] _slotElements;
        private readonly VisualElement[] _slotIcons;
        private readonly Label[] _slotAmounts;

        private DragDropManipulator _manipulator;

        public InventoryView(UIDocument document, InventoryViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = document.rootVisualElement;
            _panel = _root.Q("inventory-panel");
            _gridContainer = _root.Q("grid-container");
            _detailPanel = _root.Q("detail-panel");
            _detailName = _root.Q<Label>("detail-name");
            _detailAmount = _root.Q<Label>("detail-amount");

            var layout = _root.Q("inventory-layout");
            _layout = layout;
            var sortButton = _root.Q<Button>("sort-button");

            int slotCount = viewModel.SlotCount;
            _slotElements = new VisualElement[slotCount];
            _slotIcons = new VisualElement[slotCount];
            _slotAmounts = new Label[slotCount];

            BuildSlots(slotCount);
            SetupDragDrop();
            SetupRightClick(slotCount);

            sortButton.clicked += _viewModel.RequestSort;

            _viewModel.SlotUpdated += UpdateSlot;
            _viewModel.VisibilityChanged += OnVisibilityChanged;
            _viewModel.DetailShown += OnDetailShown;
            _viewModel.DetailHidden += OnDetailHidden;

            HideDetail();
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
            _manipulator = new DragDropManipulator(_slotElements, _root, _panel, CreateGhostElement);
            _gridContainer.AddManipulator(_manipulator);

            _manipulator.Moved += OnSlotMoved;
            _manipulator.DroppedOutside += OnSlotDroppedOutside;
            _manipulator.SlotClicked += OnSlotClicked;
        }

        private void SetupRightClick(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = i;
                _slotElements[i].RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button != 1)
                        return;

                    var modifier = ResolveDragModifier(evt);
                    int amount = _viewModel.GetMoveAmount(index, modifier);
                    _viewModel.RequestDrop(index, amount);
                    evt.StopPropagation();
                });
            }
        }

        private void OnSlotMoved(int from, int to, DragModifier modifier)
        {
            int amount = _viewModel.GetMoveAmount(from, modifier);
            _viewModel.RequestMove(from, to, amount);
        }

        private void OnSlotDroppedOutside(int from, DragModifier modifier)
        {
            int amount = _viewModel.GetMoveAmount(from, modifier);
            _viewModel.RequestDrop(from, amount);
        }

        private void OnSlotClicked(int index)
        {
            _viewModel.SelectSlot(index);
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
            if (!isOpen)
                _manipulator.CancelDrag();

            SetVisible(isOpen);
        }

        private void SetVisible(bool visible)
        {
            _layout.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDetailShown(SlotViewData data)
        {
            _detailName.text = data.Name;

            _detailAmount.text = data.Stackable
                ? $"{data.Amount} / {data.MaxStack}"
                : string.Empty;

            _detailPanel.style.display = DisplayStyle.Flex;
        }

        private void OnDetailHidden()
        {
            HideDetail();
        }

        private void HideDetail()
        {
            _detailPanel.style.display = DisplayStyle.None;
        }

        private VisualElement CreateGhostElement(int slotIndex, DragModifier modifier)
        {
            var data = _viewModel.GetSlotData(slotIndex);

            if (data.IsEmpty)
                return null;

            int moveAmount = _viewModel.GetMoveAmount(slotIndex, modifier);

            var ghost = new VisualElement();
            ghost.style.width = GhostSize;
            ghost.style.height = GhostSize;
            ghost.style.backgroundColor = data.Color;
            ghost.style.opacity = GhostOpacity;
            ghost.style.borderTopLeftRadius = GhostBorderRadius;
            ghost.style.borderTopRightRadius = GhostBorderRadius;
            ghost.style.borderBottomLeftRadius = GhostBorderRadius;
            ghost.style.borderBottomRightRadius = GhostBorderRadius;

            if (data.ShowAmount || moveAmount < data.Amount)
            {
                var label = new Label(moveAmount.ToString());
                label.pickingMode = PickingMode.Ignore;
                label.style.position = Position.Absolute;
                label.style.right = 4;
                label.style.bottom = 2;
                label.style.fontSize = GhostLabelFontSize;
                label.style.color = Color.white;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                ghost.Add(label);
            }

            return ghost;
        }

        private static DragModifier ResolveDragModifier(IPointerEvent evt)
        {
            if (evt.shiftKey) return DragModifier.HalfStack;
            if (evt.ctrlKey) return DragModifier.SingleItem;
            return DragModifier.None;
        }
    }
}
