using SCPSL.PluginListing.Enums;

namespace SCPSL.PluginListing.Models
{
    public class ReleaseInformationModel
    {
        public BuildType BuildType { get; set; } = BuildType.Release;
        public string Version { get; set; } = "1.0.0";
        public string SCPSLVersion { get; set; } = "13.4.2";
        public string FileUrl { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
    }
}
