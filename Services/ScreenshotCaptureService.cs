using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Services
{

  public interface IScreenshotCaptureService
  {
    /// <summary>
    /// Nimmt einen Screenshot in der angegebenen Region auf und gibt ihn als Bitmap zurück.
    /// </summary>
    /// <returns>Ein Bitmap-Objekt, das den Screenshot enthält.</returns>
    System.Drawing.Bitmap CaptureScreenshot (Rectangle rect);
  }

  internal class ScreenshotCaptureService : IScreenshotCaptureService
  {
    public Bitmap CaptureScreenshot (Rectangle rect)
    {
      // Create a Bitmap to hold the screenshot
      Bitmap bmp = new Bitmap (rect.Width, rect.Height);

      // Create a graphics object from the Bitmap
      using (Graphics graphics = Graphics.FromImage (bmp))
      {
        // Capture the specified area
        graphics.CopyFromScreen (rect.Left, rect.Top, 0, 0, rect.Size);
      }

      return bmp;
    }
  }
}
