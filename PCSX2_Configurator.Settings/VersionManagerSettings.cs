using System;
using System.Collections.Generic;
using System.Text;

namespace PCSX2_Configurator.Settings
{
    public class VersionManagerSettings
    {
        public string VersionsDirectory { get; private set; }
        public string ArchivesDirectory { get; private set; }
        public string BiosDirectory { get; private set; }
        public bool CopyBiosFile { get; private set; }
        public List<VersionSettings> StableVersions { get; private set; }
        public string DevVersions { get; private set; }
    }
}
