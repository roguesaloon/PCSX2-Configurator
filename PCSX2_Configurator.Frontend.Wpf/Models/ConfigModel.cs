using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;

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

        private Dictionary<string, string> remoteInfo;
        private Dictionary<string,string> RemoteInfo
        {
            get
            {
                if(IsRemote && remoteInfo == null)
                {
                    var remoteFile = $"{ConfigsDirectory}\\{Name}\\remote";
                    var remoteContents = File.ReadAllText(remoteFile);
                    remoteInfo = new Dictionary<string, string>(JsonConvert.DeserializeObject<Dictionary<string, string>>(remoteContents), StringComparer.OrdinalIgnoreCase);
                }
                return remoteInfo;
            }
        }

        public string Notes => RemoteInfo.ContainsKey("notes") ? RemoteInfo?["notes"] : null;
        public SolidColorBrush Status
        {
            get
            {
                var status = RemoteInfo.ContainsKey("status") ? RemoteInfo?["status"] : null;
                return
                    status == "g" ? new SolidColorBrush(Colors.Green) :
                    status == "y" ? new SolidColorBrush(Colors.Yellow) :
                    SystemColors.MenuBrush;
            }
        }
    }
}