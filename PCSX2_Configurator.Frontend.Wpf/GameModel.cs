using System;
using System.Collections.Generic;
using System.Text;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class GameModel
    {
        public string Game { get; set; }
        public string Path { get; set; }
        public IEnumerable<string> Versions { get; set; }
        public string Version { get; set; }
        public IEnumerable<string> Configs { get; set; }
        public string Config { get; set; }
    }
}
