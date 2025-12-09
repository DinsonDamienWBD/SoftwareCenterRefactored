using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Routing;
using SoftwareCenter.Host;
using SoftwareCenter.Host.Services;
using SoftwareCenter.Kernel;
using SoftwareCenter.UIManager;
using SoftwareCenter.UIManager.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency Injection ---
builder.Services.AddKernel();
builder.Services.AddUIManager();
builder.Services.AddSignalR(); 

// Register the notifier service that allows UIManager to talk to the hub
builder.Services.AddTransient<IUIHubNotifier, UIHubNotifier>();

var app = builder.Build();

// --- API Endpoints ---
app.MapPost("/api/dispatch/{commandName}", async (
    string commandName,
    JsonElement payload,
    ICommandBus commandBus,
    CommandFactory commandFactory) =>
{
    var commandType = commandFactory.GetCommandType(commandName);

    if (commandType == null)
    {
        return Results.NotFound($"Command '{commandName}' not found.");
    }
    
    // TODO: The ModuleId should come from an authenticated user's claims or a request header.
    var traceContext = new TraceContext { Items = { ["ModuleId"] = "Host.Frontend" } };

    try
    {
        var command = payload.Deserialize(commandType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (command == null) return Results.BadRequest("Could not deserialize command payload.");
        
        var resultInterface = commandType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
        if (resultInterface != null)
        {
            var resultType = resultInterface.GetGenericArguments()[0];
            var dispatchMethod = typeof(ICommandBus).GetMethod(nameof(ICommandBus.Dispatch)).MakeGenericMethod(resultType);
            
            var task = (Task)dispatchMethod.Invoke(commandBus, new object[] { command, traceContext });
            await task;

            var result = task.GetType().GetProperty("Result")?.GetValue(task);
            return Results.Ok(result);
        }
        else
        {
            await commandBus.Dispatch((ICommand)command, traceContext);
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        // A proper error handling middleware should be used here
        return Results.Problem(ex.InnerException?.Message ?? ex.Message, statusCode: 500);
    }
});


// --- Frontend Hosting ---
app.UseDefaultFiles();
app.UseStaticFiles();

var rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
var modulesPath = Path.Combine(rootPath, "Modules");
if (Directory.Exists(modulesPath))
{
    var moduleDirectories = Directory.GetDirectories(modulesPath);
    foreach (var moduleDir in moduleDirectories)
    {
        var moduleWwwRoot = Path.Combine(moduleDir, "wwwroot");
        if (Directory.Exists(moduleWwwRoot))
        {
            var moduleName = new DirectoryInfo(moduleDir).Name;
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(moduleWwwRoot),
                RequestPath = $"/Modules/{moduleName}"
            });
        }
    }
}

app.MapHub<UIHub>("/uihub");

// --- Initialize Host UI ---
using (var scope = app.Services.CreateScope())
{
    var commandBus = scope.ServiceProvider.GetRequiredService<ICommandBus>();
    var hostTraceContext = new TraceContext { Items = { ["ModuleId"] = "Host" } };

    // Register Nav buttons and content containers for default Host services
    await commandBus.Dispatch(
        new RegisterUIFragmentCommand(
            "<button id=\"host-apps-nav-button\" class=\"nav-button\">Applications</button>",
            parentId: "nav-zone",
            priority: HandlerPriority.Low),
        hostTraceContext);
        
    await commandBus.Dispatch(
        new RegisterUIFragmentCommand(
            "<div id=\"host-apps-content\" class=\"content-container\"><h1>Applications</h1></div>",
            parentId: "content-zone",
            priority: HandlerPriority.Low),
        hostTraceContext);
}


app.Run();