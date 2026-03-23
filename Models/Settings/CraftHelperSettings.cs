using POECraftHelper.Interfaces;
using POECraftHelper.Properties;

namespace POECraftHelper.Models
{
  public class CraftHelperSettings : ISettings
  {
    public Boolean SoundEnabled { get; set; }
    public Int32 SoundVolume { get; set; }
    public SoundType SoundType { get; set; }
    public Int32 WindowLeft { get; set; }
    public Int32 WindowTop { get; set; }
    public Dictionary<String, String> RegexPatterns { get; set; }

    public CraftHelperSettings () 
    {
      SoundEnabled = true;
      SoundVolume = 100;
      SoundType = SoundType.Divine;
      WindowLeft = 100;
      WindowTop = 100;
      RegexPatterns = new Dictionary<String, String>
      {
        { "Body Armour Regex", "^Prime|of Nullification$|of Bameth$|of the Conservator$" }
      };
    }

    void ISettings.Load ()
    {
      SoundEnabled = Properties.Settings.Default.SoundEnabled;
      SoundVolume = Properties.Settings.Default.SoundVolume;
      SoundType = (SoundType)Properties.Settings.Default.SoundType;
      WindowLeft = Properties.Settings.Default.WindowLeft;
      WindowTop = Properties.Settings.Default.WindowTop;
      RegexPatterns = RegexPatternsFromJson (Properties.Settings.Default.RegexPatternsJson);
    }

    void ISettings.Save ()
    {
      Properties.Settings.Default.SoundEnabled = SoundEnabled;
      Properties.Settings.Default.SoundVolume = SoundVolume;
      Properties.Settings.Default.SoundType = (int)SoundType;
      Properties.Settings.Default.WindowLeft = WindowLeft;
      Properties.Settings.Default.WindowTop = WindowTop;
      Properties.Settings.Default.RegexPatternsJson = RegexPatternsToJson (RegexPatterns);
    }

    private Dictionary<String, String> RegexPatternsFromJson (String x_json)
    {
      var regexPatterns = new Dictionary<String, String> ();

      if (String.IsNullOrEmpty (x_json))
        return regexPatterns;

      var jsonObject = Newtonsoft.Json.Linq.JObject.Parse (x_json);
      foreach (var property in jsonObject.Properties ())
      {
        regexPatterns[property.Name] = property.Value.ToString ();
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
