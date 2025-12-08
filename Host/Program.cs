using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwareCenter;
using SoftwareCenter.Host.Services;
using SoftwareCenter.Kernel;
using SoftwareCenter.Kernel.Commands; // Correctly reference the new command
using SoftwareCenter.Kernel.Services;
using SoftwareCenter.UI.Engine;
using SoftwareCenter.UI.Engine.Services;
using System.IO;
using System.Threading.Tasks;

namespace SoftwareCenter.Host
{
    static class Program
    {
        [System.STAThread]
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Service Registration ---
            builder.Services.AddSignalR();
            
            // Register UI Engine and Host Logger
            builder.Services.AddSingleton<IUIEngine, UIManager>();
            builder.Services.AddSingleton<IScLogger>(sp => 
                new HostLogger(Path.Combine(sp.GetRequiredService<IHostEnvironment>().ContentRootPath, "logs")));
            
            // Register Kernel and its dependencies
            builder.Services.AddSingleton<KernelLogger>();
            builder.Services.AddSingleton<IKernel>(sp => 
                new StandardKernel(sp.GetRequiredService<ILoggerFactory>()));
            // Also register the concrete type for services that might need it (like the API endpoint)
            builder.Services.AddSingleton(sp => (StandardKernel)sp.GetRequiredService<IKernel>());


            var app = builder.Build();

            // --- Logger Registration ---
            // After the app is built, find all loggers and register them with the KernelLogger
            var kernelLogger = app.Services.GetRequiredService<KernelLogger>();
            var loggers = app.Services.GetServices<IScLogger>();
            foreach (var logger in loggers)
            {
                kernelLogger.RegisterLogger(logger);
            }
            
            // --- Kernel and UI Engine Integration ---
            var kernel = app.Services.GetRequiredService<IKernel>();
            var uiEngine = app.Services.GetRequiredService<IUIEngine>();
            kernel.RegisterService<IUIEngine>(uiEngine);

            // --- API Endpoints ---
            app.MapPost("/api/ui/interact", async (InteractionRequest request, StandardKernel appKernel) =>
            {
                var command = new UIInteractionCommand(request.OwnerId, request.Action, request.AdditionalData);
                var result = await appKernel.RouteAsync(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Interaction processed." })
                    : Results.Problem(detail: result.ErrorMessage, statusCode: 500);
            });

            // --- Middleware Pipeline ---
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(); 

            var modulesPath = Path.Combine(app.Environment.ContentRootPath, "Modules");
            Directory.CreateDirectory(modulesPath);
            app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(modulesPath), RequestPath = "/modules" });

            app.MapHub<UIHub>("/ui-hub");

            // Start the Kernel (which will load modules)
            await kernel.StartAsync();

            await app.RunAsync();
        }
    }
}
