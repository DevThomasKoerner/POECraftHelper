using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using POECraftHelper.Models;

namespace POECraftHelper.Services
{
  public interface IUpdateService
  {
    String GetCurrentVersion ();
    Task<UpdateResult> CheckForUpdateAsync (CancellationToken ct = default);
    void DownloadInstaller ();
  }

  public class UpdateService : IUpdateService
  {
    private readonly HttpClient m_httpClient;

    private const string GitHubOwner = "DevThomasKoerner";
    private const string GitHubRepo  = "POECraftHelper";

    public UpdateService (HttpClient x_httpClient)
    {
      m_httpClient = x_httpClient ?? throw new ArgumentNullException (nameof (x_httpClient));
    }

    public String GetCurrentVersion ()
    {
      return GetCurrentAppVersion ();
    }


    /// <summary>
    /// Prüft, ob die laufende App der neuesten GitHub-Release entspricht.
    /// </summary>
    public async Task<UpdateResult> CheckForUpdateAsync (CancellationToken ct = default)
    {
      string currentVersion = GetCurrentAppVersion();
      string apiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases";

      using HttpResponseMessage response = await m_httpClient.GetAsync (apiUrl, ct);

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        return null;

      response.EnsureSuccessStatusCode ();

      string json = await response.Content.ReadAsStringAsync(ct);
      using JsonDocument doc = JsonDocument.Parse(json);
      JsonElement root = doc.RootElement;

      // Erstes Element = neuestes Release (inkl. Pre-Releases)
      if (!root.EnumerateArray ().Any ())
        return null;

      JsonElement latest = root.EnumerateArray().First();

      string tagName       = latest.GetProperty("tag_name").GetString() ?? string.Empty;
      string latestVersion = tagName.TrimStart('v');
      string releaseUrl    = latest.GetProperty("html_url").GetString() ?? string.Empty;
      bool isPreRelease    = latest.GetProperty("prerelease").GetBoolean();
      bool isUpToDate      = CompareVersions(currentVersion, latestVersion) >= 0;
      
      string installerUrl = "https://github.com/DevThomasKoerner/POECraftHelper/releases";

      return new UpdateResult
      {
        IsUpToDate = isUpToDate,
        CurrentVersion = currentVersion,
        LatestVersion = latestVersion,
        ReleaseUrl = releaseUrl,
        IsPreRelease = isPreRelease,
        InstallerUrl = installerUrl
      };
    }

    public async void DownloadInstaller ()
    {
      var result = await CheckForUpdateAsync ();
      if (result == null || String.IsNullOrEmpty (result.InstallerUrl) == true)
        return;

      // Öffnet den Download-Link im Standardbrowser
      Process.Start (new ProcessStartInfo
      {
        FileName = result.InstallerUrl,
        UseShellExecute = true
      });
    }

    /// <summary>
    /// Liest die Version aus der Assembly (AssemblyVersion).
    /// Fallback: "0.0.0.0"
    /// </summary>
    private String GetCurrentAppVersion ()
    {
      return Assembly.GetExecutingAssembly ()
                     .GetName ()
                     .Version
                     ?.ToString () ?? "0.0.0.0";
    }

    /// <summary>
    /// Vergleicht zwei Versions-Strings (z.B. "1.2.0.0" vs "1.3.0.0").
    /// Gibt  0 zurück wenn gleich,
    ///      >0 wenn current neuer,
    ///      <0 wenn current älter (= Update verfügbar).
    /// </summary>
    private Int32 CompareVersions (string current, string latest)
    {
      if (Version.TryParse (current, out Version v1) &&
          Version.TryParse (latest, out Version v2))
      {
        return v1.CompareTo (v2);
      }

      // Fallback: String-Vergleich
      return string.Compare (current, latest, StringComparison.OrdinalIgnoreCase);
    }

  }
}
