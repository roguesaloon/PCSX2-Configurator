using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    public interface ICoverService
    {
        public Task<string> GetCoverForGame(GameInfo game);
    }
}
