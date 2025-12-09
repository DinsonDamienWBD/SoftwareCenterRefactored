namespace SoftwareCenter.Core.Commands.UI
{
    public class UnregisterUIElementCommand : ICommand
    {
        public string ElementId { get; }

        public UnregisterUIElementCommand(string elementId)
        {
            ElementId = elementId;
        }
    }
}
