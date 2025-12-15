using SoftwareCenter.Core.Commands;

namespace SoftwareCenter.Module.ExampleModule.Commands
{
    public class SearchQueryCommand : ICommand
    {
        public string Query { get; set; }
    }
}
