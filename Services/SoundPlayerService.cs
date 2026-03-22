using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Services
{

  public interface ISoundPlayerService
  {
    void PlaySound (String x_soundName);
  }

  public class SoundPlayerService : ISoundPlayerService
  {
    public void PlaySound (String x_soundName)
    {
      var assemblyDirectory = System.IO.Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
      SoundPlayer player = null;

      switch (x_soundName)
      {
        case "Divine Sound":
          player = new SoundPlayer (System.IO.Path.Combine (assemblyDirectory, "Sounds", "Divine.wav"));
          break;
        case "Chaos Sound":
          player = new SoundPlayer (System.IO.Path.Combine (assemblyDirectory, "Sounds", "Chaos.wav"));
          break;
        case "Exalt Sound":
          player = new SoundPlayer (System.IO.Path.Combine (assemblyDirectory, "Sounds", "Exalt.wav"));
          break;
        default:
          throw new ArgumentException ($"Unbekannter Soundname: {x_soundName}");
      }

      player.Play ();
    }
  }


}
