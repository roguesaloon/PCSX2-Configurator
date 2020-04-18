using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    public interface ICoverService
    {
        public string GetCoverForGame(GameInfo game);
        public async Task<string> GetCoverForGameAsTask(GameInfo game) => await Task.Run(() => GetCoverForGame(game));
    }
}
