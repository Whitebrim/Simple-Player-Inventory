using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class DragDropManipulator : Manipulator
    {
        private const float GhostSize = 60f;
        private const float DragThreshold = 5f;

        private readonly VisualElement[] _slots;
        private readonly VisualElement _ghostLayer;
        private readonly VisualElement _panelBounds;
        private readonly Func<int, DragModifier, VisualElement> _createGhost;

        private VisualElement _ghost;
        private int _sourceIndex = -1;
        private int _pointerId = -1;
        private Vector2 _startPosition;
        private DragModifier _modifier;
        private bool _isDragging;

        public event Action<int, int, DragModifier> Moved;
        public event Action<int, DragModifier> DroppedOutside;
        public event Action<int> SlotClicked;

        public DragDropManipulator(
            VisualElement[] slots,
            VisualElement ghostLayer,
            VisualElement panelBounds,
            Func<int, DragModifier, VisualElement> createGhost)
        {
            _slots = slots;
            _ghostLayer = ghostLayer;
            _panelBounds = panelBounds;
            _createGhost = createGhost;
        }

        public void CancelDrag()
        {
            if (_sourceIndex < 0)
                return;

            if (_isDragging)
                RemoveGhost();

            if (_pointerId >= 0)
                target.ReleasePointer(_pointerId);

            ResetState();
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

            _sourceIndex = index;
            _pointerId = evt.pointerId;
            _startPosition = evt.position;
            _modifier = ResolveModifier(evt);
            _isDragging = false;

            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_sourceIndex < 0 || evt.pointerId != _pointerId)
                return;

            if (!_isDragging)
            {
                if (Vector2.Distance(evt.position, _startPosition) < DragThreshold)
                    return;

                _ghost = _createGhost(_sourceIndex, _modifier);

                if (_ghost == null)
                {
                    target.ReleasePointer(_pointerId);
                    ResetState();
                    return;
                }

                _isDragging = true;
                _ghost.style.position = Position.Absolute;
                _ghost.pickingMode = PickingMode.Ignore;
                _ghostLayer.Add(_ghost);
            }

            PositionGhost(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_sourceIndex < 0 || evt.pointerId != _pointerId)
                return;

            target.ReleasePointer(evt.pointerId);

            int sourceIndex = _sourceIndex;
            var modifier = _modifier;

            if (!_isDragging)
            {
                SlotClicked?.Invoke(sourceIndex);
                ResetState();
                evt.StopPropagation();
                return;
            }

            RemoveGhost();

            int targetIndex = FindSlotAtPosition(evt.position);
            bool insidePanel = _panelBounds.worldBound.Contains(evt.position);

            if (targetIndex >= 0 && targetIndex != sourceIndex)
                Moved?.Invoke(sourceIndex, targetIndex, modifier);
            else if (!insidePanel)
                DroppedOutside?.Invoke(sourceIndex, modifier);

            ResetState();
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
            _ghost.style.left = position.x - GhostSize * 0.5f;
            _ghost.style.top = position.y - GhostSize * 0.5f;
        }

        private void RemoveGhost()
        {
            _ghost?.RemoveFromHierarchy();
            _ghost = null;
        }

        private void ResetState()
        {
            _sourceIndex = -1;
            _pointerId = -1;
            _isDragging = false;
        }

        private static DragModifier ResolveModifier(PointerDownEvent evt)
        {
            if (evt.shiftKey) return DragModifier.HalfStack;
            if (evt.ctrlKey) return DragModifier.SingleItem;
            return DragModifier.None;
        }
    }
}
