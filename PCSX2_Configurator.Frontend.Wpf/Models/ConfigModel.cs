using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class ConfigModel
    {
        public static string ConfigsDirectory { get; private set; }
        public ConfigModel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        private bool? isRemote;
        public bool IsRemote 
        { 
            get
            {
                if(isRemote == null)
                {
                    isRemote = File.Exists($"{ConfigsDirectory}\\{Name}\\remote");
                }
                return (bool) isRemote;
            }
        }

        private List<string> gameIds;
        public List<string> GameIds 
        { 
            get
            {
                if(gameIds == null)
                {
                    var gameIdsFiles = $"{ConfigsDirectory}\\{Name}\\gameids";
                    if(File.Exists(gameIdsFiles))
                    {
                        gameIds = File.ReadAllText(gameIdsFiles).Split(';').ToList();
                    }
                }
                return gameIds;
            }
        }
    }
}
