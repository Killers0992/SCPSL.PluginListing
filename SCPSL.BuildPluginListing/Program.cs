using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Octokit;
using SCPSL.PluginListing.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

await Host.CreateDefaultBuilder()
    .RunCommandLineApplicationAsync<AppCommand>(args);

[Command(Description = "Runs listing builder.")]
public class AppCommand
{
    private GitHubClient _gitHubClient; 
    private HttpClient _http;

    [Required]
    [Option(Description = "Github Token.")]
    public string Token { get; set; } = null;

    public string Repository => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
    public string RepositoryName => Repository.Split('/')[1];
    public string RepositoryOwner => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");

    public GitHubClient Client
    {
        get
        {
            if (_gitHubClient == null)
            {
                _gitHubClient = new GitHubClient(new ProductHeaderValue("SCPSL.Plugin.Listing"));
                _gitHubClient.Credentials = new Credentials(Token);
            }

            return _gitHubClient;
        }
    }

    public HttpClient Http
    {
        get
        {
            if (_http == null)
            {
                _http = new HttpClient();
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("SCPSL Plugin Listing 1.0.0");
            }

            return _http;
        }
    }

    public async Task<int> OnExecute(IConsole console)
    {
        try
        {
            var releases = await Client.Repository.Release.GetAll(RepositoryOwner, RepositoryName);

            if (releases.Count == 0)
            {
                Console.WriteLine("No releases found, skipping!");
                return 1;
            }

            ReleasesModel releasesModel = null;

            foreach (Release release in releases)
            {
                BuildInformationModel build = null;

                string fileUrl = null;
                string fileHash = null;

                List<string> loaders = new List<string>();

                foreach (var asset in release.Assets)
                {
                    string fileName = Path.GetFileName(asset.BrowserDownloadUrl);

                    string extension = Path.GetExtension(fileName);

                    switch (extension)
                    {
                        case ".dll" when fileUrl == null:
                            using (HttpResponseMessage response = await Http.GetAsync(asset.BrowserDownloadUrl))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    byte[] data = await response.Content.ReadAsByteArrayAsync();

                                    fileHash = BytesToMD5(data);
                                    fileUrl = asset.BrowserDownloadUrl;
                                }
                            }
                            break;
                        case ".json" when fileName.ToLower() == "releaseinfo.json":
                            using(HttpResponseMessage response = await Http.GetAsync(asset.BrowserDownloadUrl))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    build = JsonConvert.DeserializeObject<BuildInformationModel>(await response.Content.ReadAsStringAsync());
                                }
                            }
                            break;
                    }
                }

                if (build == null || fileUrl == null || fileHash == null) continue;

                if (releasesModel == null)
                {
                    releasesModel = new ReleasesModel()
                    {
                        Displayname = build.Displayname,
                        Description = build.Description,
                        Author = build.AuthorName,
                        OwnerName = RepositoryOwner,
                        RepositoryName = RepositoryName,
                    };
                }

                if (releasesModel.Versions.ContainsKey(build.Version)) continue;

                releasesModel.Versions.Add(build.Version, new ReleaseInformationModel()
                {
                    Version = build.Version,
                    SCPSLVersion = build.SCPSLVersion,
                    BuildType = build.BuildType,
                    FileHash = fileHash,
                    FileUrl = fileUrl,
                });
            }

            if (releasesModel == null)
            {
                Console.WriteLine($"Repository don't have any valid releases!");
                return 1;
            }

            File.WriteAllText("./Website/releases.json", JsonConvert.SerializeObject(releasesModel, Formatting.Indented));
            Console.WriteLine("Releases file uploaded!");
            return 0;
        }
        catch (Exception ex)
        {
            console.WriteLine(ex);
            return 1;
        }
    }

    string BytesToMD5(byte[] bytes)
    {
        using (var md5 = MD5.Create())
        {
            return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}