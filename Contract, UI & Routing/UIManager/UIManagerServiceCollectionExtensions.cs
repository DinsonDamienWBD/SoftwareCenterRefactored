using Microsoft.Extensions.DependencyInjection;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.UIManager.Commands;
using SoftwareCenter.UIManager.Handlers;
using SoftwareCenter.UIManager.Services;

namespace SoftwareCenter.UIManager
{
    public static class UIManagerServiceCollectionExtensions
    {
        public static IServiceCollection AddUIManager(this IServiceCollection services)
        {
            services.AddSingleton<UIStateService>();
            services.AddTransient<ICommandHandler<CreateCardCommand>, CreateCardCommandHandler>();
            services.AddTransient<ICommandHandler<AddControlToContainerCommand>, AddControlToContainerCommandHandler>();
            return services;
        }
    }
}