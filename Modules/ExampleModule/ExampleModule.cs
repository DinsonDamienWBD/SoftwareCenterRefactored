using Microsoft.Extensions.DependencyInjection;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Modules;
using SoftwareCenter.Module.ExampleModule.Commands;
using SoftwareCenter.Module.ExampleModule.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwareCenter.Module.ExampleModule
{
    public class ExampleModule : IModule
    {
        public string Id => "ExampleModule";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, ExampleModule>();
            services.AddTransient<ICommandHandler<SearchQueryCommand, List<string>>, SearchQueryCommandHandler>();
        }

        public async Task Initialize(IServiceProvider serviceProvider)
        {
            var commandBus = serviceProvider.GetRequiredService<SoftwareCenter.Core.Commands.ICommandBus>();

            var manifest = new SoftwareCenter.Core.UI.UiManifest
            {
                Operation = "inject",
                TargetGuid = "nav-main",
                MountPoint = "default",
                RootComponent = new SoftwareCenter.Core.UI.ComponentDefinition
                {
                    Type = "tpl-search-box",
                    Content = "Search...",
                    Children = new System.Collections.Generic.List<SoftwareCenter.Core.UI.ComponentDefinition>
                    {
                        new SoftwareCenter.Core.UI.ComponentDefinition
                        {
                            Type = "tpl-button",
                            Content = "Search"
                        }
                    }
                }
            };

            await commandBus.Dispatch(new RequestUITemplateCommand
            {
                Manifest = manifest
            }, new Core.Diagnostics.TraceContext());
        }
    }
}

