namespace SCPSL.PluginListing.Models
{
    public class ReleasesModel
    {
        public string Displayname { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }

        public string OwnerName { get; set; }
        public string RepositoryName { get; set; }

        public string LoaderType { get; set; }

        public Dictionary<string, ReleaseInformationModel> Versions { get; set; } = new Dictionary<string, ReleaseInformationModel>();
    }
}
