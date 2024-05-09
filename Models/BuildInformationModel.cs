using SCPSL.PluginListing.Enums;

namespace SCPSL.PluginListing.Models
{
    public class BuildInformationModel
    {
        public string Displayname { get; set; } = "Template Plugin";
        public string Description { get; set; } = "Template plugin for testing.";
        public string AuthorName { get; set; } = "NoAuthor";
        public BuildType BuildType { get; set; } = BuildType.Release;
        public string Version { get; set; } = "1.0.0";
        public string SCPSLVersion { get; set; } = "13.4.2";
        public string LoaderType { get; set; } = "nwapi";
    }
}
