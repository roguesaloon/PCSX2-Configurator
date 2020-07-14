using System.Threading.Tasks;

namespace PCSX2_Configurator.Services
{
    interface IDiscIdLookupService
    {
        Task<string> LookupDiscId(string discId);
    }
}
