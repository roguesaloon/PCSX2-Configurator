using System.Collections.Generic;
using PCSX2_Configurator.Common;

namespace PCSX2_Configurator.Services
{
    public interface IGameLibraryService
    {
        List<GameInfo> Games { get; }
        GameInfo AddToLibrary(string isoPath);
        void UpdateGameInfo(string name, GameInfo newInfo, bool shouldReloadLibrary = false);
        void RemoveFromLibrary(GameInfo gameInfo);
    }
}
