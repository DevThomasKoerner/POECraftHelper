using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Models
{
  public enum DetectionResult
  {
    None,
    Startup,
    CalibrationRegexDetected,
    CalibrationLeftBorderError,
    CalibrationRightBorderError,
    CalibrationSuccess,
    RegexDetectionSuccess,
    RegexDetectionFailed
  }
}
