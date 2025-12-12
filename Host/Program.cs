using SoftwareCenter.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SoftwareCenter.Core;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.UI;
using SoftwareCenter.Host;
using SoftwareCenter.Host.Services;
using SoftwareCenter.Host.Hubs;
using SoftwareCenter.Kernel;
using SoftwareCenter.Kernel.Services;
using SoftwareCenter.UIManager;
using SoftwareCenter.UIManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// --- Logging Configuration ---
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new SoftwareCenter.Kernel.Logging.FileLoggerProvider(Path.Combine(AppContext.BaseDirectory, "Logs", "SoftwareCenter.log")));
builder.Logging.AddConsole();


// --- Dependency Injection ---

// Manually trigger module service configuration before building the container
var tempServices = new ServiceCollection();
tempServices.AddKernel(); // Add services needed by ModuleLoader itself
var tempProvider = tempServices.BuildServiceProvider();
var tempErrorHandler = tempProvider.GetRequiredService<SoftwareCenter.Core.Errors.IErrorHandler>();
var tempRoutingRegistry = tempProvider.GetRequiredService<IServiceRoutingRegistry>();
var tempServiceRegistry = tempProvider.GetRequiredService<IServiceRegistry>();

var moduleLoader = new ModuleLoader(tempErrorHandler, tempRoutingRegistry, tempServiceRegistry);
moduleLoader.ConfigureModuleServices(builder.Services);


builder.Services.AddKernel();
builder.Services.AddUIManager();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ITemplateService, HostTemplateService>();

// 1. Register UI Services
// We pass the WebRootPath (wwwroot) to the Template Service so it can find files.
builder.Services.AddSingleton<UiTemplateService>(sp =>
    new UiTemplateService(builder.Environment.WebRootPath));

builder.Services.AddScoped<IUiService, UiRenderer>();

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


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 2. Serve Static Files (CSS, JS, Fonts)
// IMPORTANT: We do NOT serve index.html via UseStaticFiles default behavior for root,
// because our MainController handles the root path "/".
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

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

// 3. Map Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");

app.MapHub<UIHub>("/uihub");

// --- System Initialization ---
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    // Initialize Modules
    var loader = serviceProvider.GetRequiredService<ModuleLoader>();
    await loader.InitializeModules(serviceProvider);

    // Initialize Host UI
    var commandBus = serviceProvider.GetRequiredService<ICommandBus>();
    var hostTraceContext = new TraceContext { Items = { ["ModuleId"] = "Host" } };

    // 1. Create Nav Button for "Applications"
    var navButtonId = await commandBus.Dispatch(
        new RequestUITemplateCommand(
            templateType: "nav-button",
            parentId: "nav-zone",
            initialProperties: new Dictionary<string, object>
            {
                    { "Label", "Applications" },
                    { "TargetContainerId", "host-apps-content" },
                    { "Priority", HandlerPriority.Low }
            }),
        hostTraceContext);

    // 2. Create Content Container for "Applications"
    var contentContainerId = await commandBus.Dispatch(
        new CreateUIElementCommand(
            ownerModuleId: "Host",
            parentId: "content-zone",
            elementType: SoftwareCenter.Core.UI.ElementType.Panel.ToString(), // Use Panel for a generic container
            initialProperties: new Dictionary<string, object>
            {
                    { "Id", "host-apps-content" }, // Explicitly set ID for easy targeting
                    { "Priority", HandlerPriority.Low }
            }),
        hostTraceContext);

    // 3. Inject default HTML into the "Applications" content container
    await commandBus.Dispatch(
        new RegisterUIFragmentCommand(
            htmlContent: "<h1>Applications</h1>",
            parentId: contentContainerId, // Use the ID of the newly created container
            priority: HandlerPriority.Low),
        hostTraceContext);
}

app.Run();