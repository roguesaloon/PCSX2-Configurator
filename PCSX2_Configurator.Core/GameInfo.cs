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

        private readonly string name;
        public string Name { get => name; set => ExternalSetFieldOnce(nameof(name), value); }

        private readonly string path;
        public string Path { get => path; set => ExternalSetFieldOnce(nameof(path), value); }

        private readonly string emuVersion;
        public string EmuVersion { get => emuVersion; set => ExternalSetFieldOnce(nameof(emuVersion), value); }
        
        private readonly string config;
        public string Config { get => config; set => ExternalSetFieldOnce(nameof(config), value); }

        private readonly string displayName;
        public string DisplayName { get => displayName; set => ExternalSetFieldOnce(nameof(displayName), value); }

        private readonly string region;
        public string Region { get => region; set => ExternalSetFieldOnce(nameof(region), value); }

        private readonly string gameId;
        public string GameId { get => gameId; set => ExternalSetFieldOnce(nameof(gameId), value); }

        private void ExternalSetFieldOnce(string fieldName, object value)
        {
            var field = typeof(GameInfo).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || Assembly.GetCallingAssembly() == Assembly.GetExecutingAssembly()) field.SetValue(this, value);
            else throw new Exception("Value is already set");
        }

        internal object this[string propName]
        {
            get => typeof(GameInfo).GetProperty(propName).GetValue(this);
            set => typeof(GameInfo).GetProperty(propName).SetValue(this, value);
        }
    }
}