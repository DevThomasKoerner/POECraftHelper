using POECraftHelper.Interfaces;

namespace POECraftHelper.Models
{
  public class SettingsWindowSettings : ISettings
  {
    public Double SettingsWindowHeight { get; set; }

    public Double SettingsWindowWidth { get; set; }

    public Double SettingsWindowLeft { get; set; }

    public Double SettingsWindowTop { get; set; }

    public SettingsWindowSettings ()
    {
      SettingsWindowHeight = 300;
      SettingsWindowWidth = 300;
      SettingsWindowLeft = 0;
      SettingsWindowTop = 0;
    }

    public void Load ()
    {
      SettingsWindowHeight = Properties.Settings.Default.SettingsWindowHeight;
      SettingsWindowWidth = Properties.Settings.Default.SettingsWindowWidth;
      SettingsWindowLeft = Properties.Settings.Default.SettingsWindowLeft;
      SettingsWindowTop = Properties.Settings.Default.SettingsWindowTop;
    }

    public void Save ()
    {
      Properties.Settings.Default.SettingsWindowHeight = SettingsWindowHeight;
      Properties.Settings.Default.SettingsWindowWidth = SettingsWindowWidth;
      Properties.Settings.Default.SettingsWindowLeft = SettingsWindowLeft;
      Properties.Settings.Default.SettingsWindowTop = SettingsWindowTop;
    }
  }
}
