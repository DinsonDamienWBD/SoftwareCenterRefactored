namespace SoftwareCenter.Core.Commands.UI
{
    public class RequestUITemplateCommand : ICommand<string>
    {
        public string TemplateType { get; }
        public string ParentId { get; }

        public RequestUITemplateCommand(string templateType, string parentId)
        {
            TemplateType = templateType;
            ParentId = parentId;
        }
    }
}
