using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwareCenter.Host.Services
{
    public class HostFeatureUIService : IHostedService
    {
        private readonly ICommandBus _commandBus;
        private readonly ILogger<HostFeatureUIService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public HostFeatureUIService(ICommandBus commandBus, ILogger<HostFeatureUIService> logger, IHostApplicationLifetime appLifetime)
        {
            _commandBus = commandBus;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    await InitializeHostFeaturesAsync();
                }, cancellationToken);
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task InitializeHostFeaturesAsync()
        {
            _logger.LogInformation("HostFeatureUIService is initializing host features UI.");

            var hostFeatures = new[]
            {
                new { Name = "App Manager", Id = "host-app-manager" },
                new { Name = "Source Manager", Id = "host-source-manager" }
            };

            foreach (var feature in hostFeatures)
            {
                try
                {
                    // Request Nav Button
                    var navButtonId = await _commandBus.Send<RequestUITemplateCommand, string>(new RequestUITemplateCommand(
                        templateType: "nav-button",
                        parentId: null,
                        initialProperties: new Dictionary<string, object>
                        {
                            { "SlotName", "nav-rail" },
                            { "Title", feature.Name },
                            { "TargetId", $"{feature.Id}-content" } // Convention for linking button to content
                        }
                    ));
                    _logger.LogInformation("Created nav button for {FeatureName} with ID {NavButtonId}", feature.Name, navButtonId);

                    // Request Content Panel
                    var contentPanelId = await _commandBus.Send<RequestUITemplateCommand, string>(new RequestUITemplateCommand(
                        templateType: "content-panel",
                        parentId: null, // Top-level element
                        initialProperties: new Dictionary<string, object>
                        {
                            { "Id", $"{feature.Id}-content" }, // Use the ID the button will target
                            { "SlotName", "content-area" },
                            { "Title", feature.Name }
                        }
                    ));
                    _logger.LogInformation("Created content panel for {FeatureName} with ID {ContentPanelId}", feature.Name, contentPanelId);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to create UI for host feature: {FeatureName}", feature.Name);
                }
            }
        }
    }
}
