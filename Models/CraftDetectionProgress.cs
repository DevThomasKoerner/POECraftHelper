using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Models
{
  public class CraftDetectionProgress
  {
    public DetectionResult DetectionResult { get; set; }

    public CraftDetectionProgress (DetectionResult x_detectionResult)
    {
      DetectionResult = x_detectionResult;
    }
  }
}
