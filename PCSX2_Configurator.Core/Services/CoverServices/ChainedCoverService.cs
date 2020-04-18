namespace PCSX2_Configurator.Core
{
    public sealed class ChainedCoverService : ICoverService
    {
        private readonly PlaystationDataCenterCoverService playstationDataCenterService;
        private readonly Playstation2ArchiveCoverService playstation2ArchiveService;

        public ChainedCoverService(string coversPath)
        {
            coversPath ??= "Assets/Covers";
            playstationDataCenterService = new PlaystationDataCenterCoverService(coversPath);
            playstation2ArchiveService = new Playstation2ArchiveCoverService(coversPath);
        }

        public string GetCoverForGame(GameInfo game)
        {
            var result = playstationDataCenterService.GetCoverForGame(game);
            if (!string.IsNullOrEmpty(result) && !result.ToLower().Contains("missing")) return result;

            result = playstation2ArchiveService.GetCoverForGame(game);
            return result;
        }
    }
}