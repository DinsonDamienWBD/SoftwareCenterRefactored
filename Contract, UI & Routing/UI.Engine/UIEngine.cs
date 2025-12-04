using Microsoft.AspNetCore.SignalR;
using SoftwareCenter.Core.UI;
using SoftwareCenter.Core.UI.Client;
using SoftwareCenter.Core.UI.Requests;
using SoftwareCenter.UI.Engine.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SoftwareCenter.UI.Engine;

public class UIEngine : IUIEngine
{
    private readonly IHubContext<UIHub> _hubContext;
    private readonly ConcurrentDictionary<string, UIElement> _uiState = new();

    public UIEngine(IHubContext<UIHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<string> RequestNavButtonAsync(NavButtonRequest request)
    {
        var button = new ClientNavButton
        {
            Id = $"nav-{Guid.NewGuid()}",
            Label = request.Label,
            Icon = request.Icon,
            Priority = request.Priority
        };

        _uiState.TryAdd(button.Id, new UIElement(button.Id, request.OwnerId, UIElementType.NavButton, button));

        await _hubContext.Clients.All.SendAsync("RenderNavButton", button);

        return button.Id;
    }

    public async Task<string> RequestContentContainerAsync(ContentContainerRequest request)
    {
        // Ensure the associated nav button exists and is owned by the requestor
        if (!_uiState.TryGetValue(request.AssociatedNavButtonId, out var navButton) || navButton.OwnerId != request.OwnerId)
        {
            throw new InvalidOperationException("The associated navigation button does not exist or is not owned by the requester.");
        }

        var container = new ClientContentContainer
        {
            Id = $"container-{Guid.NewGuid()}",
            OwnerId = request.OwnerId,
            AssociatedNavButtonId = request.AssociatedNavButtonId,
            HasSpa = !string.IsNullOrWhiteSpace(request.SpaUrl),
            SpaUrl = request.SpaUrl
        };

        _uiState.TryAdd(container.Id, new UIElement(container.Id, request.OwnerId, UIElementType.Container, container));

        await _hubContext.Clients.All.SendAsync("RenderContentContainer", container);

        return container.Id;
    }

    public async Task UpdateUIElementAsync(UIUpdateRequest request)
    {
        // Ensure the element exists and is owned by the requestor
        if (!_uiState.TryGetValue(request.TargetElementId, out var element) || element.OwnerId != request.OwnerId)
        {
            // Or log a warning, depending on desired strictness
            return;
        }

        await _hubContext.Clients.All.SendAsync("UpdateElementContent", request.TargetElementId, request.CustomHtmlContent ?? string.Empty);
    }

    public async Task RemoveUIElementAsync(string elementId, string ownerId)
    {
        // Ensure the element exists and is owned by the requestor
        if (!_uiState.TryGetValue(elementId, out var element) || element.OwnerId != ownerId)
        {
            return;
        }

        if (_uiState.TryRemove(elementId, out _))
        {
            await _hubContext.Clients.All.SendAsync("RemoveElement", elementId);

            // Also remove any associated containers if a nav button is removed
            if (element.Type == UIElementType.NavButton)
            {
                var associatedContainers = _uiState.Where(kvp =>
                        kvp.Value.Type == UIElementType.Container &&
                        kvp.Value.StateObject is ClientContentContainer container &&
                        container.AssociatedNavButtonId == elementId)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var containerId in associatedContainers)
                {
                    if (_uiState.TryRemove(containerId, out _))
                    {
                        await _hubContext.Clients.All.SendAsync("RemoveElement", containerId);
                    }
                }
            }
        }
    }
}