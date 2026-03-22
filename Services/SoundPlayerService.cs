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
    void PlaySoud ();
  }
  public class SoundPlayerService : ISoundPlayerService
  {
    public void PlaySoud ()
    {
      var assemblyDirectory = System.IO.Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
      SoundPlayer player = new SoundPlayer(System.IO.Path.Combine (assemblyDirectory, "Sounds", "Divine.wav"));
      player.Play ();
    }
  }


}
