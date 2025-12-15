using System.Threading.Tasks;

namespace SoftwareCenter.Host.Services
{
    public interface IHostFeatureUIService
    {
        Task InitializeHostFeaturesAsync();
    }
}
