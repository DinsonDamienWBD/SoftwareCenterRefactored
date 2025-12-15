using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Services
{
    public interface IUIHubNotifier
    {
        Task InjectFragment(string targetGuid, string mountPoint, string htmlContent);
        Task UpdateFragment(string targetGuid, string mountPoint, string htmlContent);
        Task RemoveFragment(string targetGuid);
    }
}
