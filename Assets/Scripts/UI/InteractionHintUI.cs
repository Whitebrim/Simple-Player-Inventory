using UnityEngine.UIElements;

namespace Game.UI
{
    public class InteractionHintUI
    {
        private readonly Label _hintLabel;
        private readonly VisualElement _root;

        public InteractionHintUI(UIDocument document)
        {
            _root = document.rootVisualElement;
            _hintLabel = _root.Q<Label>("hint-label");
            Hide();
        }

        public void Show(string text)
        {
            _hintLabel.text = text;
            _root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
        }
    }
}
