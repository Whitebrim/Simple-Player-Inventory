using UnityEngine.UIElements;

namespace Game.UI
{
    public class NotificationUI
    {
        private const long DisplayDurationMs = 2000;

        private readonly Label _label;
        private readonly VisualElement _root;
        private IVisualElementScheduledItem _hideTask;

        public NotificationUI(UIDocument document)
        {
            _root = document.rootVisualElement;
            _label = _root.Q<Label>("notification-label");
            Hide();
        }

        public void Show(string text)
        {
            _label.text = text;
            _root.style.display = DisplayStyle.Flex;

            _hideTask?.Pause();
            _hideTask = _root.schedule.Execute(Hide).StartingIn(DisplayDurationMs);
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
        }
    }
}
