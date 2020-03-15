namespace PCSX2_Configurator.Core
{
    public class GameInfo
    {
        public GameInfo() { }

        private GameInfo(GameInfo gameInfo)
        {
            foreach (var prop in typeof(GameInfo).GetProperties())
            {
                this[prop.Name] = gameInfo[prop.Name];
            }
        }

        public GameInfo
            (GameInfo gameInfo, string name = null , string path = null, string emuVersion = null , string config = null, string displayName = null, string region = null, string gameId = null) 
            : this(gameInfo) => SetFromValues(name, path, emuVersion, config, displayName, region, gameId);

        public GameInfo
            (string name, string path, string emuVersion, string config = null, string displayName = null, string region = null, string gameId = null) 
            => SetFromValues(name, path, emuVersion, config, displayName, region, gameId);

        private void SetFromValues(string name, string path, string emuVersion, string config, string displayName, string region, string gameId)
        {
            Name = name ?? Name;
            Path = path ?? Path;
            EmuVersion = emuVersion ?? EmuVersion;
            Config = config ?? Config;
            DisplayName = displayName ?? DisplayName;
            Region = region ?? Region;
            GameId = gameId ?? GameId;
        }

        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public string EmuVersion { get; internal set; }
        public string Config { get; internal set; }
        public string DisplayName { get; internal set; }
        public string Region { get; internal set; }
        public string GameId { get; internal set; }

        internal object this[string propName]
        {
            get => typeof(GameInfo).GetProperty(propName).GetValue(this);
            set => typeof(GameInfo).GetProperty(propName).SetValue(this, value);
        }
    }
}
