using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace PCSX2_Configurator.Core
{
    public sealed class EmulationService
    {
        private readonly IProcessHelpers processHelpers;
        public EmulationService()
        {
            processHelpers = new WindowsProcessHelpers();
        }

        public static void LaunchWithGame(string emulatorPath, string gamePath, string configPath)
        {
            Process.Start(new ProcessStartInfo(emulatorPath, $"\"{gamePath}\" --fullscreen --nogui --cfgpath=\"{configPath}\"")
            {
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            });
        }

        public static void LaunchWithConfig(string emulatorPath, string configPath)
        {
            Process.Start(new ProcessStartInfo(emulatorPath, $"--cfgpath=\"{configPath}\"")
            {
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            });
        }

        public static string GetInisPath(string emulatorPath)
        {
            var inisPath = $"{Path.GetDirectoryName(emulatorPath)}\\inis";
            inisPath = emulatorPath.Contains("1.4.0") ? inisPath.Replace("\\inis", "\\inis_1.4.0") : inisPath;
            return inisPath;
        }

        public (string gameTitle, string gameRegion, string gameId) IdentifyGame(string emulatorPath, string gamePath)
        {
            var gameTitle = default(string);
            var gameRegion = default(string);
            var gameId = default(string);
            var pcsx2Directory = Path.GetDirectoryName(emulatorPath);
            var startInfo = new ProcessStartInfo(emulatorPath, $"\"{gamePath}\" --windowed --nogui --console --gs=\"{pcsx2Directory}\\plugins\\GSnull.dll\"")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized,
                WorkingDirectory = pcsx2Directory
            };

            using (var process = Process.Start(startInfo))
            {
                var retryCount = 4;
                for (int i = 0; i < retryCount; ++i)
                {
                    Thread.Sleep(2000);
                    var windowTitle = processHelpers.GetWindowTitleText(process.MainWindowHandle);
                    var idMatch = Regex.Match(windowTitle, "\\[[A-Z]{4}-[0-9]{5}]");
                    if (idMatch.Success)
                    {
                        var regionMatch = Regex.Match(windowTitle, "\\(.*?\\)");
                        gameTitle = windowTitle.Substring(0, regionMatch.Index).Trim();
                        gameRegion = regionMatch.Value.Substring(1, regionMatch.Value.Length - 2);
                        gameId = idMatch.Value.Substring(1, 10);
                        break;
                    }
                }
                process.Kill();
            }

            return (gameTitle, gameRegion, gameId);
        }
    }
}
