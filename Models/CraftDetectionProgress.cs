using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Models
{
  public class CraftDetectionProgress
  {
    public Boolean RegexHit { get; set; }

    public CraftDetectionProgress (Boolean x_regexHit)
    {
      RegexHit = x_regexHit;
    }
  }
}
