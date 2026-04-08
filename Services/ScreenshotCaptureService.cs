using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using POECraftHelper.Models;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

namespace POECraftHelper.Services
{

  public interface IScreenshotCaptureService
  {
    void Initialize (Rectangle x_captureRegion);

    /// <summary>
    /// Nimmt einen Screenshot in der angegebenen Region auf und gibt ihn als Bitmap zurück.
    /// </summary>
    /// <returns>Ein Bitmap-Objekt, das den Screenshot enthält.</returns>
    Mat CaptureScreenshot (Rectangle x_captureRegion);
  }

  internal class ScreenshotCaptureDirectXService : IScreenshotCaptureService, IDisposable
  {
    private readonly ILoggingService m_loggingService;

    private ID3D11Device m_device;
    private ID3D11DeviceContext m_context;
    private IDXGIOutputDuplication m_duplication;
    private ID3D11Texture2D m_stagingTexture;
    private OutputDescription m_outputDesc;

    private Rectangle m_lastRegion;
    private bool m_disposed;

    // Fehlercodes für DXGI
    private const int DXGI_ERROR_ACCESS_LOST = unchecked((int)0x887A0026);
    private const int DXGI_WAIT_TIMEOUT = unchecked((int)0x887A0027);

    public ScreenshotCaptureDirectXService (ILoggingService loggingService)
    {
      m_loggingService = loggingService;
    }

    public void Initialize (Rectangle x_captureRegion)
    {
      ThrowIfDisposed ();
      m_lastRegion = x_captureRegion;
      SetupDirectX ();

      // WARM-UP: Warte bis zu 500ms auf den ersten gültigen Frame
      var sw = Stopwatch.StartNew();
      while (sw.ElapsedMilliseconds < 500)
      {
        using var firstFrame = CaptureScreenshot(x_captureRegion);
        if (firstFrame != null && !IsImageEmpty (firstFrame))
          break; // Wir haben ein echtes Bild!

        Thread.Sleep (10);
      }
    }

    private bool IsImageEmpty (Mat mat)
    {
      // Einfacher Check: Ist der Mittelwert der Pixel fast 0? (Schwarz)
      MCvScalar mean = CvInvoke.Mean(mat);
      return mean.V0 < 1.0 && mean.V1 < 1.0 && mean.V2 < 1.0;
    }

    private void SetupDirectX ()
    {
      try
      {
        CleanupResources ();

        // 1. Device erstellen
        D3D11.D3D11CreateDevice (null, DriverType.Hardware, DeviceCreationFlags.None, null, out m_device, out m_context).CheckError ();

        // 2. Den richtigen Monitor (Output) finden
        using var dxgiDevice = m_device.QueryInterface<IDXGIDevice>();
        using var adapter = dxgiDevice.GetAdapter();

        // Standardmäßig nehmen wir Output 0 (Hauptmonitor)
        // In einem Multi-Monitor-Setup müsste hier der Index gewählt werden, auf dem m_lastRegion liegt
        adapter.EnumOutputs (0, out var output).CheckError ();
        using var output1 = output.QueryInterface<IDXGIOutput1>();
        m_outputDesc = output1.Description;

        // 3. Duplication starten
        m_duplication = output1.DuplicateOutput (m_device);

        // 4. Staging Texture für CPU-Read erstellen (einmalig in der Größe der Region)
        var desc = new Texture2DDescription
        {
          Width = (uint)m_lastRegion.Width,
          Height = (uint)m_lastRegion.Height,
          ArraySize = 1,
          MipLevels = 1,
          Format = Format.B8G8R8A8_UNorm,
          SampleDescription = new SampleDescription(1, 0),
          Usage = ResourceUsage.Staging,
          BindFlags = BindFlags.None,
          CPUAccessFlags = CpuAccessFlags.Read
        };
        m_stagingTexture = m_device.CreateTexture2D (desc);
      }
      catch (Exception ex)
      {
        m_loggingService?.Log ($"DXGI Setup Failed: {ex.Message}");
        throw;
      }
    }

    public Mat CaptureScreenshot (Rectangle region)
    {
      ThrowIfDisposed ();

      if (m_duplication == null)
        return null;

      IDXGIResource screenResource = null;
      try
      {
        // Frame holen (kurzer Timeout für hohe Frameraten)
        var result = m_duplication.AcquireNextFrame(50, out var frameInfo, out screenResource);

        using (screenResource)
        using (var texture = screenResource.QueryInterface<ID3D11Texture2D> ())
        {
          // BOX Berechnung: Wichtig für die korrekte Position!
          // Die Koordinaten müssen RELATIV zum Desktop-Ursprung des Monitors sein.
          // Wenn Monitor bei 1920 startet und Region bei 2000, ist Left = 80.
          var box = new Vortice.Mathematics.Box
          {
            Left = Math.Max(0, region.Left - m_outputDesc.DesktopCoordinates.Left),
            Top = Math.Max(0, region.Top - m_outputDesc.DesktopCoordinates.Top),
            Front = 0,
            Right = Math.Max(0, region.Right - m_outputDesc.DesktopCoordinates.Left),
            Bottom = Math.Max(0, region.Bottom - m_outputDesc.DesktopCoordinates.Top),
            Back = 1
          };

          m_context.CopySubresourceRegion (m_stagingTexture, 0, 0, 0, 0, texture, 0, box);
        }

        m_duplication.ReleaseFrame ();
        return ProcessStagingTexture (region);
      }
      catch (SharpGen.Runtime.SharpGenException ex) when (ex.ResultCode == DXGI_WAIT_TIMEOUT)
      {
        // Statisches Bild: Gib einfach den letzten Inhalt der Staging Texture zurück
        return ProcessStagingTexture (region);
      }
      catch (SharpGen.Runtime.SharpGenException ex) when (ex.ResultCode == DXGI_ERROR_ACCESS_LOST)
      {
        m_loggingService?.Log ("DXGI Access Lost (Alt-Tab or UAC). Re-initializing...");
        SetupDirectX (); // Automatischer Recovery
        return null;
      }
      catch (Exception ex)
      {
        m_loggingService?.Log ($"Capture Error: {ex.Message}");
        return null;
      }
    }

    private Mat ProcessStagingTexture (Rectangle region)
    {
      if (m_stagingTexture == null)
        return null;

      var map = m_context.Map(m_stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
      try
      {
        // BGRA (DirectX Standard) -> BGR (OpenCV Standard)
        // Wir erstellen eine Kopie, damit der Pointer nach Unmap valide bleibt
        using var matBgra = new Mat(region.Height, region.Width, DepthType.Cv8U, 4, map.DataPointer, (int)map.RowPitch);
        var matBgr = new Mat();
        CvInvoke.CvtColor (matBgra, matBgr, ColorConversion.Bgra2Bgr);
        return matBgr;
      }
      finally
      {
        m_context.Unmap (m_stagingTexture, 0);
      }
    }

    private void CleanupResources ()
    {
      m_stagingTexture?.Dispose ();
      m_duplication?.Dispose ();
      m_context?.Dispose ();
      m_device?.Dispose ();

      m_stagingTexture = null;
      m_duplication = null;
      m_context = null;
      m_device = null;
    }

    private void ThrowIfDisposed ()
    {
      if (m_disposed)
        throw new ObjectDisposedException (nameof (ScreenshotCaptureDirectXService));
    }

    public void Dispose ()
    {
      if (m_disposed)
        return;
      CleanupResources ();
      m_disposed = true;
    }
  }

  internal class ScreenshotCaptureService : IScreenshotCaptureService, IDisposable
  {
    private Boolean m_disposed = false;

    private Bitmap m_buffer;

    private Graphics m_graphics;



    public Mat CaptureScreenshot (Rectangle rect)
    {
      ThrowIfDisposed ();

      // Screenshot im angegebenen Bereich aufnehmen
      m_graphics.CopyFromScreen (rect.Left, rect.Top, 0, 0, rect.Size);

      // Das Bitmap zurückgeben, das den Screenshot enthält
      Mat mat = m_buffer.ToMat();
      return mat;
    }

    private void ThrowIfDisposed ()
    {
      if (m_disposed)
        throw new ObjectDisposedException (nameof (ScreenshotCaptureService));
    }

    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    protected virtual void Dispose (Boolean x_disposing)
    {
      if (m_disposed)
        return;

      if (x_disposing)
      {
        // Managed + unmanaged Ressourcen freigeben
        m_graphics?.Dispose ();
        m_buffer?.Dispose ();
      }

      m_graphics = null;
      m_buffer = null;

      m_disposed = true;
    }

    public void Initialize (Rectangle x_rect)
    {
      if (m_buffer == null || m_buffer.Width != x_rect.Width || m_buffer.Height != x_rect.Height)
      {
        m_buffer?.Dispose ();
        m_buffer = new Bitmap (x_rect.Width, x_rect.Height);
        m_graphics = Graphics.FromImage (m_buffer);
      }
    }
  }
}
