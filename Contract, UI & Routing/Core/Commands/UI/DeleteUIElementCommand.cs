namespace SoftwareCenter.Core.Commands.UI
{
    public class DeleteUIElementCommand : ICommand
    {
        public string ElementId { get; }

        public DeleteUIElementCommand(string elementId)
        {
            ElementId = elementId;
        }
    }
}
