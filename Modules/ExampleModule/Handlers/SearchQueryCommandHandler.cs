using SoftwareCenter.Core.Commands;
using SoftwareCenter.Module.ExampleModule.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwareCenter.Module.ExampleModule.Handlers
{
    public class SearchQueryCommandHandler : ICommandHandler<SearchQueryCommand, List<string>>
    {
        public Task<List<string>> Handle(SearchQueryCommand command, Core.Diagnostics.ITraceContext traceContext)
        {
            var suggestions = new List<string>
            {
                "Apple",
                "Banana",
                "Orange"
            };

            var result = suggestions.FindAll(s => s.ToLower().Contains(command.Query.ToLower()));

            return Task.FromResult(result);
        }
    }
}
