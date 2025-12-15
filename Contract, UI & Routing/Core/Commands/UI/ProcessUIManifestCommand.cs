using SoftwareCenter.Core.UI;

namespace SoftwareCenter.Core.Commands.UI
{
    public class ProcessUIManifestCommand : ICommand
    {
        public UiManifest Manifest { get; }

        public ProcessUIManifestCommand(UiManifest manifest)
        {
            Manifest = manifest;
        }
    }
}
