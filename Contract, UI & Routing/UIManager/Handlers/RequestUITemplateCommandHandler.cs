using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Data.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Routing;
using SoftwareCenter.UIManager.Services;
using System;
using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Handlers
{
    public class RequestUITemplateCommandHandler : ICommandHandler<RequestUITemplateCommand, string>
    {
        private readonly UIStateService _uiStateService;
        private readonly IUIHubNotifier _hubNotifier;

        public RequestUITemplateCommandHandler(UIStateService uiStateService, IUIHubNotifier hubNotifier)
        {
            _uiStateService = uiStateService;
            _hubNotifier = hubNotifier;
        }

        public async Task<string> Handle(RequestUITemplateCommand command, TraceContext traceContext)
        {
            if (command.TemplateType.Equals("Card", StringComparison.OrdinalIgnoreCase))
            {
                var elementId = $"card_{Guid.NewGuid().ToString("N")}";
                var ownerId = traceContext.ModuleId;

                var cardHtml = @"<div class=""card""></div>";

                var uiElement = new UIElement(elementId, ownerId, "Card", command.ParentId);

                var fragment = new UIFragment
                {
                    Id = elementId,
                    OwnerId = ownerId,
                    HtmlContent = cardHtml,
                    Priority = HandlerPriority.Normal, 
                    SlotName = null, // Cards don't override each other, they are added to a parent
                    Element = uiElement
                };

                if (_uiStateService.TryAddFragment(fragment))
                {
                    await _hubNotifier.ElementAdded(new
                    {
                        ElementId = fragment.Id,
                        ParentId = command.ParentId,
                        HtmlContent = fragment.HtmlContent,
                    });

                    return elementId;
                }
            }
            
            // Handle other template types or failure
            return null;
        }
    }
}
