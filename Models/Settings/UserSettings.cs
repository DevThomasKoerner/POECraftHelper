using POECraftHelper.Interfaces;

namespace POECraftHelper.Models
{
  public class UserSettings : ISettings
  {
    public Boolean SoundEnabled { get; set; }
    public Int32 SoundVolume { get; set; }
    public SoundType SoundType { get; set; }
    public Dictionary<String, String> RegexPatterns { get; set; }
    public Boolean IsSoundExpanded { get; set; }
    public Boolean IsRegexExpanded { get; set; }


    public UserSettings () 
    {
      SoundEnabled = true;
      SoundVolume = 100;
      SoundType = SoundType.Divine;
      RegexPatterns = new Dictionary<String, String>
      {
        { "Body Armour", "^Prime|of Nullification$|of Bameth$|of the Conservator$" }
      };
      IsSoundExpanded = true;
      IsRegexExpanded = true;
    }

    void ISettings.Load ()
    {
      SoundEnabled = Properties.Settings.Default.SoundEnabled;
      SoundVolume = Properties.Settings.Default.SoundVolume;
      SoundType = (SoundType)Properties.Settings.Default.SoundType;
      RegexPatterns = RegexPatternsFromJson (Properties.Settings.Default.RegexPatternsJson);
      IsSoundExpanded = Properties.Settings.Default.IsSoundExpanded;
      IsRegexExpanded = Properties.Settings.Default.IsRegexExpanded;
    }

    void ISettings.Save ()
    {
      Properties.Settings.Default.SoundEnabled = SoundEnabled;
      Properties.Settings.Default.SoundVolume = SoundVolume;
      Properties.Settings.Default.SoundType = (int)SoundType;
      Properties.Settings.Default.RegexPatternsJson = RegexPatternsToJson (RegexPatterns);
      Properties.Settings.Default.IsSoundExpanded = IsSoundExpanded;
      Properties.Settings.Default.IsRegexExpanded = IsRegexExpanded;
    }

    private Dictionary<String, String> RegexPatternsFromJson (String x_json)
    {
      var regexPatterns = new Dictionary<String, String> ();

      if (String.IsNullOrEmpty (x_json))
      {
        // Füge ein Beispiel-Regex-Muster hinzu, falls keine Regex vorhanden ist.
        regexPatterns["Body Armour"] = "^Prime|of Nullification$|of Bameth$|of the Conservator$";
        return regexPatterns;
      }
       
      var jsonObject = Newtonsoft.Json.Linq.JObject.Parse (x_json);
      foreach (var property in jsonObject.Properties ())
      {
        regexPatterns[property.Name] = property.Value.ToString ();
      }

      // Füge ein Beispiel-Regex-Muster hinzu, falls keine Regex vorhanden ist.
      if (regexPatterns.Count == 0)
      {
        regexPatterns["Body Armour"] = "^Prime|of Nullification$|of Bameth$|of the Conservator$";
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
