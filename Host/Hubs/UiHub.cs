using Microsoft.AspNetCore.SignalR;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Kernel.Services;
using System.Threading.Tasks;

namespace SoftwareCenter.Host.Hubs
{
    public class UiHub : Hub
    {
        private readonly ICommandBus _commandBus;
        private readonly CommandFactory _commandFactory;

        public UiHub(ICommandBus commandBus, CommandFactory commandFactory)
        {
            _commandBus = commandBus;
            _commandFactory = commandFactory;
        }

        public async Task RouteToModule(string moduleId, string commandName, object payload)
        {
            var commandType = _commandFactory.GetCommandType(commandName);
            if (commandType == null)
            {
                // Optionally, send an error back to the client
                return;
            }

            var command = System.Text.Json.JsonSerializer.Deserialize(payload.ToString(), commandType, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var traceContext = new TraceContext();
            traceContext.Items["ModuleId"] = moduleId;
            
            await _commandBus.Dispatch((ICommand)command, traceContext);
        }
    }
}