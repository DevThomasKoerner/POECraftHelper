using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace POECraftHelper.Services
{
  public interface ICraftDetectionService
  {
    Boolean DetectCraft (Rectangle x_craftWindowRectangle);
  }

  internal class CraftDetectionService : ICraftDetectionService
  {
    private readonly IScreenshotCaptureService m_screenCaptureService;

    public CraftDetectionService (IScreenshotCaptureService x_screenshotCaptureService)
    {
      m_screenCaptureService = x_screenshotCaptureService;
    }

    public Boolean DetectCraft (Rectangle x_craftWindowRectangle)
    {
      using (var bmp = m_screenCaptureService.CaptureScreenshot (x_craftWindowRectangle))
      using (Mat colorImage = bmp.ToMat ())
      using (Mat grayImage = new Mat ())
      using (Mat maskImage = new Mat ())
      {
        CvInvoke.CvtColor (colorImage, grayImage, ColorConversion.Bgr2Gray);
        CvInvoke.InRange (grayImage, new ScalarArray (188), new ScalarArray (188), maskImage);
        int count = CvInvoke.CountNonZero (maskImage);

        if (count > 15)
          return true;

        return false;
      }
    }
  }
}
