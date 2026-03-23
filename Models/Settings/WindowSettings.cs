using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Interfaces;

namespace POECraftHelper.Models
{
  public class WindowSettings : ISettings
  {
    public Double SettingsWindowHeight { get; set; }

    public Double SettingsWindowWidth { get; set; }

    public WindowSettings ()
    {
      SettingsWindowHeight = 300;
      SettingsWindowWidth = 300;
    }

    public void Load ()
    {
      SettingsWindowHeight = Properties.Settings.Default.SettingsWindowHeight;
      SettingsWindowWidth = Properties.Settings.Default.SettingsWindowWidth;
    }

    public void Save ()
    {
      Properties.Settings.Default.SettingsWindowHeight = SettingsWindowHeight;
      Properties.Settings.Default.SettingsWindowWidth = SettingsWindowWidth;
    }
  }
}
