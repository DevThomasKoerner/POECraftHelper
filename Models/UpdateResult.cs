using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Models
{
  public class UpdateResult
  {
    public bool IsUpToDate { get; init; }
    public string CurrentVersion { get; init; } = string.Empty;
    public string LatestVersion { get; init; } = string.Empty;
    public string ReleaseUrl { get; init; } = string.Empty;
    public bool IsPreRelease { get; init; }
    public string InstallerUrl { get; set; } = string.Empty;
  }
}
