using System;
using System.Reflection;

namespace PCSX2_Configurator.Core
{
    public class GameInfo
    {
        public GameInfo() { }

        public GameInfo(GameInfo gameInfo)
        {
            foreach (var prop in typeof(GameInfo).GetProperties())
            {
                this[prop.Name] = gameInfo[prop.Name];
            }
        }

        private string name;
        public string Name { get => name; set => name = this[nameof(name), value] as string; }

        private string path;
        public string Path { get => path; set => path = this[nameof(path), value] as string; }

        private string emuVersion;
        public string EmuVersion { get => emuVersion; set => emuVersion = this[nameof(emuVersion), value] as string; }

        private string launchOptions;
        public string LaunchOptions { get => launchOptions; set => launchOptions = this[nameof(launchOptions), value] as string; }

        private string config;
        public string Config { get => config; set => config = this[nameof(config), value] as string; }

        private string displayName;
        public string DisplayName { get => displayName; set => displayName = this[nameof(displayName), value] as string; }

        private string region;
        public string Region { get => region; set => region = this[nameof(region), value] as string; }

        private string gameId;
        public string GameId { get => gameId; set => gameId = this[nameof(gameId), value] as string; }

        private object this[string fieldName, object value]
        {
            get
            {
                var field = typeof(GameInfo).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null || Assembly.GetCallingAssembly() == Assembly.GetExecutingAssembly()) field.SetValue(this, value);
                else throw new Exception("Value is already set");
                return value;
            }
        }

        internal object this[string propName]
        {
            get => typeof(GameInfo).GetProperty(propName).GetValue(this);
            set => typeof(GameInfo).GetProperty(propName).SetValue(this, value);
        }
    }
}