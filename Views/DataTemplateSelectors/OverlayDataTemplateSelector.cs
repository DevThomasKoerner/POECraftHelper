using System.Windows;
using System.Windows.Controls;
using POECraftHelper.Models;

namespace POECraftHelper.Views
{
  public class OverlayDataTemplateSelector : DataTemplateSelector
  {
    public DataTemplate CalibrationRegexDetectedTemplate { get; set; }
    public DataTemplate CalibrationBorderErrorTemplate { get; set; }
    public DataTemplate RegexDetectionSuccessTemplate { get; set; }

    public override DataTemplate SelectTemplate (object item, DependencyObject container)
    {
      if (item is not DetectionResult detectionResult)
        return base.SelectTemplate (item, container);

      switch (detectionResult)
      {
        case DetectionResult.CalibrationRegexDetected:
          return CalibrationRegexDetectedTemplate;

        case DetectionResult.CalibrationLeftBorderError:
          return CalibrationBorderErrorTemplate;

        case DetectionResult.CalibrationRightBorderError:
          return CalibrationBorderErrorTemplate;

        case DetectionResult.RegexDetectionSuccess:
          return RegexDetectionSuccessTemplate;
      }

      return base.SelectTemplate (item, container);
    }
  }
}
