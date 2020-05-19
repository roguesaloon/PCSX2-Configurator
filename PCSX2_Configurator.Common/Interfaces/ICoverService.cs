using System.Threading.Tasks;
using PCSX2_Configurator.Common;

namespace PCSX2_Configurator.Services
{
    public interface ICoverService
    {
        Task<string> GetCoverForGame(GameInfo game);
    }
}
