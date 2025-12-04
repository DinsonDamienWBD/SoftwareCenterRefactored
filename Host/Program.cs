using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SoftwareCenter.Core.UI;
using SoftwareCenter.UI.Engine;
using SoftwareCenter.UI.Engine.Services;
using System.IO;

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

            var app = builder.Build();

            // --- Middleware Pipeline ---
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles(); // Enables serving index.html for root requests
            app.UseStaticFiles(); // Serve files from wwwroot

            // Convention for serving module SPAs
            // Assumes modules are in a 'Modules' subfolder of the Host's output directory
            var modulesPath = Path.Combine(app.Environment.ContentRootPath, "Modules");
            if (Directory.Exists(modulesPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(modulesPath),
                    RequestPath = "/modules" // Maps requests to /modules/... to the Modules folder
                });
            }

            app.MapHub<UIHub>("/ui-hub");

            await app.RunAsync();
        }
    }
}
