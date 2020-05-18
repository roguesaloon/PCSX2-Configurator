using System.Threading.Tasks;

namespace PCSX2_Configurator.Core.Services
{
    public interface ICoverService
    {
        public Task<string> GetCoverForGame(GameInfo game);
    }
}
