using System.Collections.Generic;

namespace SoftwareCenter.UIManager.Services
{
    public class UIStateService
    {
        private readonly Dictionary<string, List<string>> _cards = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> _controls = new Dictionary<string, List<string>>();

        public void CreateCard(string containerId, string title)
        {
            if (!_cards.ContainsKey(containerId))
            {
                _cards[containerId] = new List<string>();
            }
            _cards[containerId].Add(title);
        }

        public void AddControlToContainer(string containerId, string controlType, string controlId)
        {
            if (!_controls.ContainsKey(containerId))
            {
                _controls[containerId] = new List<string>();
            }
            _controls[containerId].Add($"{controlType}:{controlId}");
        }

        public Dictionary<string, List<string>> GetCards()
        {
            return _cards;
        }

        public Dictionary<string, List<string>> GetControls()
        {
            return _controls;
        }
    }
}