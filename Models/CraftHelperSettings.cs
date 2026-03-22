using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Models
{
  public class CraftHelperSettings
  {
    public Boolean SoundEnabled { get; set; }

    public Int32 SoundVolume { get; set; }

    public SoundType SoundType { get; set; }

    public Int32 WindowLeft;

    public Int32 WindowTop;

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
  }
}
