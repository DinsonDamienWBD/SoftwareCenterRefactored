using SoftwareCenter.Core.Routing;

namespace SoftwareCenter.Core.Commands.UI
{
    public class RegisterUIFragmentCommand : ICommand<string>
    {
        public string? ParentId { get; }
        public string? SlotName { get; }
        public string HtmlContent { get; }
        public string? CssContent { get; }
        public string? JsContent { get; }
        public HandlerPriority Priority { get; }

        public RegisterUIFragmentCommand(string htmlContent, string? parentId = null, string? slotName = null, string? cssContent = null, string? jsContent = null, HandlerPriority priority = HandlerPriority.Normal)
        {
            ParentId = parentId;
            SlotName = slotName;
            HtmlContent = htmlContent;
            CssContent = cssContent;
            JsContent = jsContent;
            Priority = priority;
        }
    }
}
