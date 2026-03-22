using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Models;

namespace POECraftHelper.Services
{

  public interface ISoundPlayerService
  {
    void PlaySound (SoundType x_soundType);
  }

  public class SoundPlayerService : ISoundPlayerService
  {
    public void PlaySound (SoundType x_soundType)
    {
      var assemblyDirectory = System.IO.Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

      SoundPlayer player = null;
      switch (x_soundType)
      {
        case SoundType.Divine:
          player = new SoundPlayer (System.IO.Path.Combine (assemblyDirectory, "Sounds", $"{SoundType.Divine.ToString ()}.wav"));
          break;
        case SoundType.Chaos:
          player = new SoundPlayer (System.IO.Path.Combine (assemblyDirectory, "Sounds", $"{SoundType.Chaos.ToString ()}.wav"));
          break;
        case SoundType.Exalt:
          player = new SoundPlayer (System.IO.Path.Combine (assemblyDirectory, "Sounds", $"{SoundType.Exalt.ToString ()}.wav"));
          break;

        default:
          throw new ArgumentException ($"Sound type {x_soundType} is not supported.");
      }

      player.Play ();
    }
  }


}
