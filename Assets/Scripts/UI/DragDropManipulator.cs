using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class DragDropManipulator : Manipulator
    {
        private readonly VisualElement[] _slots;
        private readonly VisualElement _ghostLayer;
        private readonly Func<int, VisualElement> _createGhost;

        private VisualElement _ghost;
        private int _sourceIndex = -1;
        private int _pointerId = -1;

        public event Action<int, int> Moved;
        public event Action<int> DroppedOutside;

        public DragDropManipulator(
            VisualElement[] slots,
            VisualElement ghostLayer,
            Func<int, VisualElement> createGhost)
        {
            _slots = slots;
            _ghostLayer = ghostLayer;
            _createGhost = createGhost;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || _sourceIndex >= 0)
                return;

            int index = FindSlotAtPosition(evt.position);

            if (index < 0)
                return;

            var ghost = _createGhost(index);

            if (ghost == null)
                return;

            _sourceIndex = index;
            _pointerId = evt.pointerId;
            _ghost = ghost;

            _ghost.style.position = Position.Absolute;
            _ghost.pickingMode = PickingMode.Ignore;
            PositionGhost(evt.position);
            _ghostLayer.Add(_ghost);

            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_ghost == null || evt.pointerId != _pointerId)
                return;

            PositionGhost(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_sourceIndex < 0 || evt.pointerId != _pointerId)
                return;

            target.ReleasePointer(evt.pointerId);

            if (_ghost != null)
            {
                _ghost.RemoveFromHierarchy();
                _ghost = null;
            }

            int targetIndex = FindSlotAtPosition(evt.position);

            if (targetIndex >= 0 && targetIndex != _sourceIndex)
                Moved?.Invoke(_sourceIndex, targetIndex);
            else if (targetIndex < 0)
                DroppedOutside?.Invoke(_sourceIndex);

            _sourceIndex = -1;
            _pointerId = -1;

            evt.StopPropagation();
        }

        private int FindSlotAtPosition(Vector2 panelPosition)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].worldBound.Contains(panelPosition))
                    return i;
            }

            return -1;
        }

        private void PositionGhost(Vector2 position)
        {
            const float ghostSize = 60f;
            _ghost.style.left = position.x - ghostSize * 0.5f;
            _ghost.style.top = position.y - ghostSize * 0.5f;
        }
    }
}
