using System.Collections.Generic;

namespace SoftwareCenter.Core.Commands.UI
{
    public class UpdateUIElementCommand : ICommand
    {
        public string ElementId { get; }
        public string? HtmlContent { get; }
        public Dictionary<string, string>? AttributesToSet { get; }
        public List<string>? AttributesToRemove { get; }


        public UpdateUIElementCommand(string elementId, string? htmlContent = null, Dictionary<string, string>? attributesToSet = null, List<string>? attributesToRemove = null)
        {
            ElementId = elementId;
            HtmlContent = htmlContent;
            AttributesToSet = attributesToSet;
            AttributesToRemove = attributesToRemove;
        }
    }
}