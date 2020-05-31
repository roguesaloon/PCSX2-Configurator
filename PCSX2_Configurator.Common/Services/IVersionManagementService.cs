using System.Collections.Generic;
using System.Threading.Tasks;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Services
{
    public interface IVersionManagementService
    {
        Task<IDictionary<string, VersionSettings>> GetAvailableVersions();
        Task InstallVersion(VersionSettings version);
        string GetMostRecentStableVersion(IEnumerable<string> versionNames);
    }
}
