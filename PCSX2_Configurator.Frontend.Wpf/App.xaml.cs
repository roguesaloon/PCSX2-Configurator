using System.IO;
using System.Reflection;
using System.Windows;

namespace PCSX2_Configurator.MainUi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var thirdPartyAssembliesDirectory = $"{Directory.GetCurrentDirectory()}\\ThirdParty";
            if (Directory.Exists(thirdPartyAssembliesDirectory))
            {
                foreach (var assembly in Directory.GetFiles(thirdPartyAssembliesDirectory, "*dll"))
                {
                    Assembly.LoadFrom(assembly);
                }
            }
        }
    }
}
