using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.UI;
using SoftwareCenter.UI.Engine;
using SoftwareCenter.UI.Engine.Services;
using System.IO;
using System.Text.Json;

namespace SoftwareCenter.Host
{
    static class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Service Registration ---
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUIEngine, UIEngine>();
            
            // Create and register the Kernel instance
            var kernel = new StandardKernel();
            builder.Services.AddSingleton(kernel);

            var app = builder.Build();

            // --- Kernel and UI Engine Integration ---
            // After the app is built, retrieve the IUIEngine and register it with the Kernel's DI container.
            // This makes the IUIEngine available to all modules loaded by the Kernel.
            var uiEngine = app.Services.GetRequiredService<IUIEngine>();
            kernel.RegisterService<IUIEngine>(uiEngine);

            // --- API Endpoints ---
            app.MapPost("/api/ui/interact", async (HttpContext context, StandardKernel appKernel) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(body) ?? new Dictionary<string, object>();

                if (!parameters.TryGetValue("ownerId", out var ownerIdObj) || ownerIdObj is not JsonElement ownerIdElem || ownerIdElem.GetString() is not string ownerId ||
                    !parameters.TryGetValue("action", out var actionObj) || actionObj is not JsonElement actionElem || actionElem.GetString() is not string action)
                {
                    return Results.BadRequest("Request must include 'ownerId' and 'action'.");
                }

                var command = new UIInteractionCommand(ownerId, action, parameters);
                var result = await appKernel.RouteAsync(command);

                // Return a generic success or failure. The module is responsible for any UI updates.
                return result.IsSuccess
                    ? Results.Ok(new { message = "Interaction processed." })
                    : Results.Problem(detail: result.ErrorMessage, statusCode: 500);
            });

            // --- Middleware Pipeline ---
            if (app.Environment.IsDevelopment())

            app.MapHub<UIHub>("/ui-hub");

            // Start the Kernel (which will load modules)
            await kernel.StartAsync();

            await app.RunAsync();
        }
    }
}

