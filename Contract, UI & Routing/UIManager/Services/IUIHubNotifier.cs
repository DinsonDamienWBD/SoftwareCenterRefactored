using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Services
{
    public interface IUIHubNotifier
    {
        Task ElementAdded(object elementData);
        Task ElementUpdated(object elementData);
        Task ElementRemoved(object elementData);
    }
}
