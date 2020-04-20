using System;
using System.Collections.Generic;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class GameModel : ICloneable
    {
        public string Game { get; set; }
        public string Path { get; set; }
        public IEnumerable<string> Versions { get; set; }
        public string Version { get; set; }
        public IEnumerable<string> Configs { get; set; }
        public string Config { get; set; }
        public string CoverPath { get; set; }

        object ICloneable.Clone() => Clone();
        public GameModel Clone() => (GameModel) MemberwiseClone();
    }
}
