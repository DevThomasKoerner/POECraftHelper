using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using POECraftHelper.Models;

namespace POECraftHelper.Services
{
  public interface IRegexDetectionService
  {
    void InitializeRun (Rectangle x_rect);

    DetectionResult Calibrate (Rectangle x_rect);

    DetectionResult Detect (Rectangle x_rect);
  }

  internal class RegexDetectionService : IRegexDetectionService
  {
    private readonly IScreenshotCaptureService m_screenCaptureService;

    //private Int32 m_calibratedXLeftBorder = -1;

    //private Int32 m_calibratedXRightBorder = -1;

    private ScalarArray m_regexScalar;

    private Mat m_mask;

    public RegexDetectionService (IScreenshotCaptureService x_screenshotCaptureService)
    {
      m_screenCaptureService = x_screenshotCaptureService;
    }

    public void InitializeRun (Rectangle x_rect)
    {
      var regexBGR = new MCvScalar (119, 180, 231, 255);
      m_regexScalar = new ScalarArray (regexBGR);

      // Maske für die Erkennung anlegen.
      m_mask?.Dispose ();
      m_mask = new Mat (x_rect.Height, x_rect.Width, DepthType.Cv8U, 1);

      m_screenCaptureService.Initialize (x_rect);
    }

    public DetectionResult Calibrate (Rectangle x_rect)
    {
      //m_calibratedXLeftBorder = -1;
      //m_calibratedXRightBorder = -1;

      using (Mat colorImage = m_screenCaptureService.CaptureScreenshot (x_rect))
      using (Mat grayImage = new Mat ())
      using (Mat mask = new Mat ())
      {
        if (IsRegexBorderVisible (colorImage) == true)
          return DetectionResult.CalibrationRegexDetected;

        CvInvoke.CvtColor (colorImage, grayImage, ColorConversion.Bgr2Gray);

        // Bild in der Mitte vertikal teilen, um die linke und rechte Hälfte separat zu analysieren.
        Rectangle leftRect = new Rectangle (0, 0, grayImage.Width / 2, grayImage.Height);
        Rectangle rightRect = new Rectangle (grayImage.Width / 2, 0, grayImage.Width / 2, grayImage.Height);

        using (Mat leftHalf = new Mat (grayImage, leftRect))
        using (Mat rightHalf = new Mat (grayImage, rightRect))
        using (Mat flippedRightHalf = new Mat ())
        using (Mat leftGradientImage = new Mat ())
        using (Mat rightGradientImage = new Mat ())
        using (Mat leftBinaryImage = new Mat ())
        using (Mat rightBinaryImage = new Mat ())
        {
          CvInvoke.Flip (rightHalf, flippedRightHalf, FlipType.Horizontal);

          CvInvoke.Sobel (leftHalf, leftGradientImage, DepthType.Cv16S, 1, 0, 3);
          CvInvoke.Sobel (flippedRightHalf, rightGradientImage, DepthType.Cv16S, 1, 0, 3);

          CvInvoke.Threshold (leftGradientImage, leftBinaryImage, 20, 255, ThresholdType.Binary);
          CvInvoke.Threshold (rightGradientImage, rightBinaryImage, 20, 255, ThresholdType.Binary);

          var whitePixelsLeft = GetWhitePixelCountPerColumn (leftBinaryImage);
          var whitePixelsRight = GetWhitePixelCountPerColumn (rightBinaryImage);

          if (IsBorderVisible (whitePixelsLeft, leftBinaryImage.Width, leftBinaryImage.Height, true) == false)
            return DetectionResult.CalibrationLeftBorderError;

          if (IsBorderVisible (whitePixelsRight, rightBinaryImage.Width, rightBinaryImage.Height, false) == false)
            return DetectionResult.CalibrationRightBorderError;
        }

        return DetectionResult.CalibrationSuccess;
      }
    }

    private Boolean IsBorderVisible (int[] x_whitePixelsPerColumn, int x_imageWidth, int x_imageHeight, Boolean x_isLeftImage)
    {
      // Die erste Spalte von rechts finden, die mindestens 80% weiße Pixel hat
      int threshold = (int)(x_imageHeight * 0.8);
      for (int x = x_imageWidth - 1; x >= 0; x--)
      {
        if (x_whitePixelsPerColumn[x] >= threshold)
        {
          // Position merken
          //if (x_isLeftImage == true)
          //  m_calibratedXLeftBorder = x;
          //else
          //  m_calibratedXRightBorder = x;

          return true;
        }
      }

      return false;
    }


    private Boolean IsRegexBorderVisible (Mat x_colorImage)
    {
      CvInvoke.InRange (x_colorImage, m_regexScalar, m_regexScalar, m_mask);

      using (Mat columnSums = new Mat ())
      {
        CvInvoke.Reduce (m_mask, columnSums, ReduceDimension.SingleRow, ReduceType.ReduceSum, DepthType.Cv32S);

        int height = x_colorImage.Rows;
        int threshold = (int) (height * 0.7 * 255);

        unsafe
        {
          int* data = (int*)columnSums.DataPointer;
          int width = columnSums.Cols;

          int count = 0;
          for (int i = 0; i < width; i++)
          {
            if (data[i] >= threshold)
            {
              var number = data[i];

              count++;
              if (count >= 2)
                return true;
            }
          }
        }

        return false;
      }
    }

    private Int32[] GetWhitePixelCountPerColumn (Mat x_binaryImage)
    {
      int width = x_binaryImage.Width;
      int height = x_binaryImage.Height;

      int[] whitePixelCountPerColumn = new int[width];
      using (var img = x_binaryImage.ToImage<Gray, byte> ())
      {
        for (int x = 0; x < width; x++)
        {
          int count = 0;

          for (int y = 0; y < height; y++)
          {
            if (img.Data[y, x, 0] == 255)
              count++;
          }

          whitePixelCountPerColumn[x] = count;
        }
      }

      return whitePixelCountPerColumn;
    }

    public DetectionResult Detect (Rectangle x_rect)
    {
      using (Mat colorImage = m_screenCaptureService.CaptureScreenshot (x_rect))
      //using (Mat colorImage = bmp.ToMat ())
      {
        if (IsRegexBorderVisible (colorImage) == true)
          return DetectionResult.RegexDetectionSuccess;
        
        return DetectionResult.RegexDetectionFailed;
      }
    }
  }
}
