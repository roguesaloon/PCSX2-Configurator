using PCSX2_Configurator.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Services
{
    internal sealed class Iso7zIdentificationService : IIdentificationService
    {
        public Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
        {
            throw new NotImplementedException();
        }

        public Task ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, Action<GameInfo, string> callback, Action deferredCallback)
        {
            throw new NotImplementedException();
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
        internal static void SetLibraryPath(string libraryPath) => LoadLibrary($"{libraryPath}\\Iso7z.dll");
    }
}