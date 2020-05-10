using System.IO;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class GameLibraryService
    {
        public List<GameInfo> Games { get; private set; }

        private readonly XmlDocument xmlDocument;
        private readonly string targetFile;
        private readonly string defaultLaunchOptions;
        
        public GameLibraryService(AppSettings appSettings)
        {
            targetFile = appSettings.GameLibraryFile ?? "GameLibrary.xml";
            defaultLaunchOptions = appSettings.DefaultLaunchOptions;
            xmlDocument = new XmlDocument();

            if (!File.Exists(targetFile))
            {
                xmlDocument.AppendChild(xmlDocument.CreateElement("Games"));
                xmlDocument.Save(targetFile);
            }
            LoadOrReloadFromLibrary();
        }

        public GameInfo AddToLibrary(string isoPath)
        {
            var gameNode = xmlDocument.CreateElement("Game");
            var pathNode = xmlDocument.CreateElement("Path");
            var launchOptionsNode = xmlDocument.CreateElement("LaunchOptions");
            var gameName = xmlDocument.CreateAttribute("Name");

            pathNode.InnerText = isoPath;
            gameNode.AppendChild(pathNode);
            launchOptionsNode.InnerText = defaultLaunchOptions;
            gameNode.AppendChild(launchOptionsNode);
            gameName.Value = Path.GetFileNameWithoutExtension(isoPath);
            gameNode.Attributes.Append(gameName);
            
            var existingNode = xmlDocument.SelectSingleNode($"//Game[@Name=\"{gameName.Value}\"]");
            if(existingNode != null) xmlDocument.DocumentElement.ReplaceChild(gameNode, existingNode);
            else xmlDocument.DocumentElement.AppendChild(gameNode);
            xmlDocument.Save(targetFile);
            LoadOrReloadFromLibrary();
            return Games.FirstOrDefault(game => game.Name == gameName.Value);
        }

        public void UpdateGameInfo(GameInfo gameInfo, GameInfo newInfo, bool shouldReloadLibrary = false)
        {
            if (!Games.Contains(gameInfo)) return;
            var gamePath = $"//Game[@Name=\"{gameInfo.Name}\"]";
            var gameNode = xmlDocument.SelectSingleNode(gamePath);

            var props = typeof(GameInfo).GetProperties().Where(_ => _.Name != "Name");
            foreach(var prop in props)
            {
                if (gameInfo[prop.Name] == newInfo[prop.Name]) continue;
                gameInfo[prop.Name] = newInfo[prop.Name];
                var nodeOfProp = xmlDocument.SelectSingleNode(gamePath + "/" + prop.Name);
                if (nodeOfProp != null) gameNode.RemoveChild(nodeOfProp);
                nodeOfProp = xmlDocument.CreateElement(prop.Name);
                nodeOfProp.InnerText = gameInfo[prop.Name].ToString();
                gameNode.AppendChild(nodeOfProp);
            }
            xmlDocument.Save(targetFile);
            if(shouldReloadLibrary) LoadOrReloadFromLibrary();
        }

        public void RemoveFromLibrary(GameInfo gameInfo)
        {
            if (!Games.Contains(gameInfo)) return;
            var gamePath = $"//Game[@Name=\"{gameInfo.Name}\"]";
            var gameNode = xmlDocument.SelectSingleNode(gamePath);

            xmlDocument.DocumentElement.RemoveChild(gameNode);
            xmlDocument.Save(targetFile);
            LoadOrReloadFromLibrary();
        }

        private void LoadOrReloadFromLibrary()
        {
            Games = new List<GameInfo>();
            xmlDocument.Load(targetFile);
            foreach (XmlElement node in xmlDocument.DocumentElement.ChildNodes)
            {
                if (node.Name == "Game")
                {
                    var gameName = node.Attributes["Name"].Value;
                    var existing = Games.FirstOrDefault(game => game.Name == gameName);
                    if (existing != null) Games.Remove(existing);
                    var gameInfo = new GameInfo { Name = gameName };
                    foreach (var prop in typeof(GameInfo).GetProperties().Where(_ => _.Name != "Name"))
                    {
                        gameInfo[prop.Name] = xmlDocument.SelectSingleNode($"//Game[@Name=\"{gameName}\"]/{prop.Name}")?.InnerText;
                    }
                    Games.Add(gameInfo);
                }
            }
        }
    }
}
