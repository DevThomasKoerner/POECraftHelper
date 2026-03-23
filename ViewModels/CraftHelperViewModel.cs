using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using POECraftHelper.Core;
using POECraftHelper.Models;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class CraftHelperViewModel : ObservableObject, IDisposable
  {
    #region Services

    private readonly IRegexDetectionService m_regexDetectionService;

    private readonly IWindowService m_windowService;

    private readonly ILoggingService m_loggingService;

    private readonly ISoundPlayerService m_soundPlayerService;

    private readonly ISettingsService m_settingsService;

    #endregion

    #region Private Fields

    private CancellationTokenSource m_cancellationTokenSource;

    private CraftHelperSettings m_currentSettings;

    #endregion

    #region Binding Properties

    private Boolean m_isRunning;

    public Boolean IsRunning
    {
      get => m_isRunning;
      set
      {
        m_isRunning = value;
        OnPropertyChanged (nameof (IsRunning));
        OnPropertyChanged (nameof (StartStopText));
        OnPropertyChanged (nameof (CanSettings));
      }
    }

    private String m_startStopText;

    public String StartStopText
    {
      get => IsRunning ? "Stop" : "Start";
      set
      {
        m_startStopText = value;
        OnPropertyChanged (nameof (StartStopText));
      }
    }

    private Double m_windowHeight = 272;
    public Double WindowHeight
    {
      get => m_windowHeight;
      set
      {
        m_windowHeight = value;
        OnPropertyChanged (nameof (WindowHeight));
      }
    }

    private Double m_windowWidth = 206;
    public Double WindowWidth
    {
      get => m_windowWidth;
      set
      {
        m_windowWidth = value;
        OnPropertyChanged (nameof (WindowWidth));
      }
    }

    private Double m_windowLeft = 274;
    public Double WindowLeft
    {
      get => m_windowLeft;
      set
      {
        m_windowLeft = value;
        OnPropertyChanged (nameof (WindowLeft));
      }
    }

    private Double m_windowTop = 348;
    public Double WindowTop
    {
      get => m_windowTop;
      set
      {
        m_windowTop = value;
        OnPropertyChanged (nameof (WindowTop));
      }
    }

    #endregion



    #region Commands

    public ICommand StartStopCommand { get; }
    public ICommand SettingsCommand { get; }

    #endregion

    #region CanProperties

    public Boolean CanStartStop => (m_windowService.IsSettingsOpen () == false);

    public Boolean CanSettings => (IsRunning == false) && (m_windowService.IsSettingsOpen () == false);

    #endregion


    public CraftHelperViewModel (IRegexDetectionService x_regexDetectionService, 
                                 IWindowService x_windowService, 
                                 ILoggingService x_loggingService,
                                 ISoundPlayerService x_soundPlayerService,
                                 ISettingsService x_settingsService)
    {
      m_regexDetectionService = x_regexDetectionService;
      m_windowService = x_windowService;
      m_loggingService = x_loggingService;
      m_soundPlayerService = x_soundPlayerService;
      m_settingsService = x_settingsService;

      StartStopCommand = new RelayCommand (OnStartStop);
      SettingsCommand = new RelayCommand<Window> (OnSettings);
    }

    private async void OnStartStop ()
    {
      if (IsRunning == true)
      {
        m_cancellationTokenSource?.Cancel ();
        return;
      }

      try
      {
        IsRunning = true;

        m_cancellationTokenSource = new CancellationTokenSource ();

        var craftDetectionProgress = new Progress<CraftDetectionProgress> (OnProgressChanged);

        m_currentSettings = m_settingsService.LoadSettings<CraftHelperSettings> ();

        m_windowService.Initialize ();

        var dpi = VisualTreeHelper.GetDpi (Application.Current.MainWindow);

        var rect = new Rectangle ((Int32)((WindowLeft + 20) * dpi.DpiScaleX),
                                  (Int32)(WindowTop * dpi.DpiScaleY),
                                  (Int32)(WindowWidth * dpi.DpiScaleX),
                                  (Int32)(WindowHeight * dpi.DpiScaleY));

        await Task.Run (() => StartAsync (m_cancellationTokenSource.Token, craftDetectionProgress, rect));
      }
      catch (OperationCanceledException)
      {
        m_loggingService.Log ($"Operation was cancelled by the user.");
      }
      catch (Exception ex)
      {
        m_loggingService.Log ($"Error: {ex.Message}");
      }
      finally
      {
        IsRunning = false;
      }
    }

    private void StartAsync (CancellationToken cancellationToken, IProgress<CraftDetectionProgress> x_progressReporter, Rectangle x_rect)
    {
      while (cancellationToken.IsCancellationRequested == false)
      {
        if (m_regexDetectionService.Detect (x_rect) == true)
        {
          x_progressReporter.Report (new CraftDetectionProgress (true));
          break;
        }
      }
    }

    private void OnProgressChanged (CraftDetectionProgress x_progress)
    {
      if (x_progress.RegexHit == true)
      {
        m_windowService.ShowOverlay (new Rect (WindowLeft, WindowTop, WindowWidth, WindowHeight));

        m_soundPlayerService.PlaySound (m_currentSettings.SoundType);

        m_loggingService.Log ("Regex hit.");
      }
    }

    private void OnSettings (Window x_window)
    {
      if (m_windowService.IsSettingsOpen () == true)
        return;

      m_windowService.ShowSettings (x_window);

      m_windowService.SettingWindowClosed += OnSettingsClosed;

      // GUI aktualisieren.
      OnPropertyChanged (nameof (CanSettings));
      OnPropertyChanged (nameof (CanStartStop));
    }

    private void OnSettingsClosed (Object sender, EventArgs e)
    {
      // GUI aktualisieren.
      OnPropertyChanged (nameof (CanSettings));
      OnPropertyChanged (nameof (CanStartStop));

      m_windowService.SettingWindowClosed -= OnSettingsClosed;
    }


    public void Dispose ()
    {
      if (m_cancellationTokenSource != null)
      {
        m_cancellationTokenSource.Cancel ();
        m_cancellationTokenSource.Dispose ();
        m_cancellationTokenSource = null;
      }
    }
  }
}
