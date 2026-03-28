using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Interfaces;

namespace POECraftHelper.Models
{
  public class MainWindowSettings : ISettings
  {
    public Double MainWindowHeight { get; set; }

    public Double MainWindowWidth { get; set; }

    public Double MainWindowLeft { get; set; }

    public Double MainWindowTop { get; set; }

    public MainWindowSettings ()
    {
      MainWindowHeight = 255;
      MainWindowWidth = 245;
      MainWindowLeft = 238;
      MainWindowTop = 374;
    }

    public void Load ()
    {
      MainWindowHeight = Properties.Settings.Default.MainWindowHeight;
      MainWindowWidth = Properties.Settings.Default.MainWindowWidth;
      MainWindowLeft = Properties.Settings.Default.MainWindowLeft;
      MainWindowTop = Properties.Settings.Default.MainWindowTop;
    }

    public void Save ()
    {
      Properties.Settings.Default.MainWindowHeight = MainWindowHeight;
      Properties.Settings.Default.MainWindowWidth = MainWindowWidth;
      Properties.Settings.Default.MainWindowLeft = MainWindowLeft;
      Properties.Settings.Default.MainWindowTop = MainWindowTop;
    }
  }
}
