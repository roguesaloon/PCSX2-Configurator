using System.IO;

namespace PCSX2_Configurator.Settings
{
    public class CoverSettings
    {
        private string coversPath;
        public string CoversPath { get => coversPath; private set => coversPath = Path.GetFullPath(value); }

        private string missingCover;
        public string MissingCover { get => missingCover; private set => missingCover = Path.GetFullPath(value); }

        private string loadingCover;
        public string LoadingCover { get => loadingCover; private set => loadingCover = Path.GetFullPath(value); }
    }
}
