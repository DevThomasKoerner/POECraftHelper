using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace POECraftHelper.Services
{
  public interface IRegexDetectionService
  {
    Boolean Detect (Rectangle x_rect);
  }

  internal class RegexDetectionService : IRegexDetectionService
  {
    private readonly IScreenshotCaptureService m_screenCaptureService;

    public RegexDetectionService (IScreenshotCaptureService x_screenshotCaptureService)
    {
      m_screenCaptureService = x_screenshotCaptureService;
    }

    public Boolean Detect (Rectangle x_rect)
    {
      using (var bmp = m_screenCaptureService.CaptureScreenshot (x_rect))
      using (Mat colorImage = bmp.ToMat ())
      using (Mat grayImage = new Mat ())
      using (Mat maskImage = new Mat ())
      {
        CvInvoke.CvtColor (colorImage, grayImage, ColorConversion.Bgr2Gray);
        CvInvoke.InRange (grayImage, new ScalarArray (188), new ScalarArray (188), maskImage);

        colorImage.Save ("c:\\temp\\colorImage.png");
        int count = CvInvoke.CountNonZero (maskImage);

        if (count > 100)
          return true;

        return false;
      }
    }
  }
}
