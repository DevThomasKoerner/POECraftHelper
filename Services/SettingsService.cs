using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Models;

namespace POECraftHelper.Services
{
  public interface ISettingsService
  {
    CraftHelperSettings LoadSettings ();

    void SaveSettings (CraftHelperSettings x_appSettings);
  }

  public class SettingsService : ISettingsService
  {
    private readonly ILoggingService m_loggingService;

    public SettingsService (ILoggingService x_loggingService)
    {
      m_loggingService = x_loggingService;
    }

    public CraftHelperSettings LoadSettings ()
    {
      var settings = new CraftHelperSettings ();

      settings.SoundEnabled = Properties.Settings.Default.SoundEnabled;
      settings.SoundVolume = Properties.Settings.Default.SoundVolume;
      settings.SoundType = (SoundType)Properties.Settings.Default.SoundType;
      settings.WindowLeft = Properties.Settings.Default.WindowLeft;
      settings.WindowTop = Properties.Settings.Default.WindowTop;
      settings.RegexPatterns = RegexPatternsFromJson (Properties.Settings.Default.RegexPatternsJson);

      return settings;
    }

    public void SaveSettings (CraftHelperSettings x_craftHelperSettings)
    {
      Properties.Settings.Default.SoundEnabled = x_craftHelperSettings.SoundEnabled;
      Properties.Settings.Default.SoundVolume = x_craftHelperSettings.SoundVolume;
      Properties.Settings.Default.SoundType = (Int32)x_craftHelperSettings.SoundType;
      Properties.Settings.Default.WindowLeft = x_craftHelperSettings.WindowLeft;
      Properties.Settings.Default.WindowTop = x_craftHelperSettings.WindowTop;
      Properties.Settings.Default.RegexPatternsJson = RegexPatternsToJson (x_craftHelperSettings.RegexPatterns);

      Properties.Settings.Default.Save ();
    }

    private Dictionary<String, String> RegexPatternsFromJson (String x_json)
    {
      var regexPatterns = new Dictionary<String, String> ();

      if (String.IsNullOrEmpty (x_json))
        return regexPatterns;

      try
      {
        var jsonObject = Newtonsoft.Json.Linq.JObject.Parse (x_json);
        foreach (var property in jsonObject.Properties ())
        {
          regexPatterns[property.Name] = property.Value.ToString ();
        }
      }
      catch (Exception ex)
      {
        m_loggingService.Log ($"Error parsing regex patterns from JSON: {ex.Message}");
      }

      return regexPatterns;
    }

    private String RegexPatternsToJson (Dictionary<String, String> x_regexPatterns)
    {
      if (String.IsNullOrEmpty (x_regexPatterns?.ToString ()))
        return String.Empty;

      var jsonObject = new Newtonsoft.Json.Linq.JObject ();

      foreach (var kvp in x_regexPatterns)
      {
        jsonObject[kvp.Key] = kvp.Value;
      }

      return jsonObject.ToString ();
    }
  }
}
