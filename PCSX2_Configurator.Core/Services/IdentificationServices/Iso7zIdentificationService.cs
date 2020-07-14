using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PCSX2_Configurator.Services
{
    internal sealed class Iso7zIdentificationService : BaseIdentificationService
    {
        private readonly IDiscIdLookupService discIdLookupService;
        
        public Iso7zIdentificationService(IGameLibraryService gameLibraryService, ICoverService coverService, IRemoteConfigService remoteConfigService, IDiscIdLookupService discIdLookupService) : 
            base(gameLibraryService, coverService, remoteConfigService)
        {
            this.discIdLookupService = discIdLookupService;
        }

        [DllImport("Iso7z.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private extern static int PS_CreateInfo(string fileName, out IntPtr info);

        [DllImport("Iso7z.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static int PS_DestroyInfo(IntPtr info);

        [StructLayout(LayoutKind.Sequential)]
        private struct Info
        {
            public uint size;
            [MarshalAs(UnmanagedType.LPWStr)] public string bootName, bootComment;
            public uint peType;
        }

        public override async Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
        {
            var gameTitle = Path.GetFileNameWithoutExtension(gamePath);
            var gameRegion = default(string);
            var gameId = default(string);

            var createInfo = PS_CreateInfo(gamePath, out var infoPtr);
            if (createInfo == 0)
            {
                var info = Marshal.PtrToStructure<Info>(infoPtr);
                gameId = info.bootName.Replace("_", "-").Replace(".", "").ToUpperInvariant();

                gameRegion =
                    new char[] { 'P', 'A', 'K' }.Any(x => gameId[2] == x) ? "NTSC-J" :
                    gameId[2] == 'E' ? "PAL-E" : gameId[2] == 'U' ? "NTSC-U" : "???";

                gameTitle = await discIdLookupService.LookupDiscId(gameId);
            }

            PS_DestroyInfo(infoPtr);
            return (gameTitle, gameRegion, gameId);
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, int dwFlags);
        internal static void SetLibraryPath(string libraryPath) => LoadLibraryEx($"{libraryPath}\\Iso7z.dll", IntPtr.Zero, 0);
    }
}