using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using POECraftHelper.Core;
using POECraftHelper.Dialogs;
using POECraftHelper.Interfaces;
using POECraftHelper.Models;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class CraftHelperViewModel : ObservableObject, IDisposable
  {
    #region Services

    private readonly IRegexDetectionService m_regexDetectionService;

    private readonly IDialogService m_dialogService;

    private readonly ILoggingService m_loggingService;

    private readonly ISoundPlayerService m_soundPlayerService;

    private readonly ISettingsService m_settingsService;

    private readonly IApplicationService m_applicationService;

    #endregion

    #region Private Fields

    private CancellationTokenSource m_cancellationTokenSource;

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
        OnPropertyChanged (nameof (CanExit));
        OnPropertyChanged (nameof (CanMinimize));
        OnPropertyChanged (nameof (CanDrag));
        OnPropertyChanged (nameof (CanResize));
      }
    }

    private Boolean m_isOverlayVisible;
    public Boolean IsOverlayVisible
    {
      get => m_isOverlayVisible;
      set
      {
        m_isOverlayVisible = value;
        OnPropertyChanged (nameof (IsOverlayVisible));
      }
    }

    private DetectionResult m_currentDetectionResult = DetectionResult.None;
    public DetectionResult CurrentDetectionResult
    {
      get => m_currentDetectionResult;
      set
      {
        if (m_currentDetectionResult != value)
        {
          m_currentDetectionResult = value;
          OnPropertyChanged (nameof (CurrentDetectionResult));
        }
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

    private Double m_windowHeight;
    public Double WindowHeight
    {
      get => m_windowHeight;
      set
      {
        m_windowHeight = value;
        OnPropertyChanged (nameof (WindowHeight));
      }
    }

    private Double m_windowWidth;
    public Double WindowWidth
    {
      get => m_windowWidth;
      set
      {
        m_windowWidth = value;
        OnPropertyChanged (nameof (WindowWidth));
      }
    }

    private Double m_windowLeft;
    public Double WindowLeft
    {
      get => m_windowLeft;
      set
      {
        m_windowLeft = value;
        OnPropertyChanged (nameof (WindowLeft));
      }
    }

    private Double m_windowTop;
    public Double WindowTop
    {
      get => m_windowTop;
      set
      {
        m_windowTop = value;
        OnPropertyChanged (nameof (WindowTop));
      }
    }

    private Boolean m_isOkayEnabled;
    public Boolean IsOkayEnabled
    {
      get => m_isOkayEnabled;
      set
      {
        m_isOkayEnabled = value;
        OnPropertyChanged (nameof (IsOkayEnabled));
      }
    }

    private Rectangle m_detectionArea;

    public Rectangle DetectionArea
    {
      get => m_detectionArea;
      set
      {
        m_detectionArea = value;
        OnPropertyChanged (nameof (DetectionArea));
      }
    }

    #endregion

    #region Commands

    public ICommand StartStopCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand MinimizeCommand { get; }
    public ICommand OkayCommand { get; }

    #endregion

    #region CanProperties

    public Boolean CanStartStop => (IsRunning == false);

    public Boolean CanSettings => (IsRunning == false);

    public Boolean CanExit => (IsRunning == false);

    public Boolean CanMinimize => (IsRunning == false);

    public Boolean CanDrag => (IsRunning == false);

    public Boolean CanResize => (IsRunning == false);

    #endregion

    public CraftHelperViewModel (IRegexDetectionService x_regexDetectionService, 
                                 IDialogService x_dialogService, 
                                 ILoggingService x_loggingService,
                                 ISoundPlayerService x_soundPlayerService,
                                 ISettingsService x_settingsService,
                                 IApplicationService x_applicationService)
    {
      m_regexDetectionService = x_regexDetectionService ?? throw new ArgumentNullException (nameof (x_regexDetectionService));
      m_dialogService = x_dialogService ?? throw new ArgumentNullException (nameof (x_dialogService));
      m_loggingService = x_loggingService ?? throw new ArgumentNullException (nameof (x_loggingService));
      m_soundPlayerService = x_soundPlayerService ?? throw new ArgumentNullException (nameof (x_soundPlayerService));
      m_settingsService = x_settingsService ?? throw new ArgumentNullException (nameof (x_settingsService));
      m_applicationService = x_applicationService ?? throw new ArgumentNullException (nameof (x_applicationService));

      StartStopCommand = new RelayCommand (OnStartStop);
      SettingsCommand = new RelayCommand<Window> (OnSettings);
      ExitCommand = new RelayCommand (OnExit);
      MinimizeCommand = new RelayCommand (OnMinimize);
      OkayCommand = new RelayCommand (OnOkay);

      Initilize ();
    }

    private void Initilize ()
    {
      var windowSettings = m_settingsService.LoadSettings<MainWindowSettings> ();
      WindowTop = windowSettings.MainWindowTop;
      WindowLeft = windowSettings.MainWindowLeft;
      WindowHeight = windowSettings.MainWindowHeight;
      WindowWidth = windowSettings.MainWindowWidth;

      m_loggingService.Log ("Startup of POE CraftHelper");
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

        // CancellationTokenSource erstellen, damit der Prozess bei Bedarf abgebrochen werden kann.
        m_cancellationTokenSource = new CancellationTokenSource ();

        // Progress-Reporter erstellen.
        var craftDetectionProgress = new Progress<CraftDetectionProgress> (OnProgressChanged);

        // Detektionsprozess starten.
        await Task.Run (() => StartDetectionAsync (m_cancellationTokenSource.Token, craftDetectionProgress, DetectionArea));
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
        m_cancellationTokenSource?.Dispose ();
        m_cancellationTokenSource = null;
      }
    }

    private async Task StartDetectionAsync (CancellationToken cancellationToken, IProgress<CraftDetectionProgress> x_progressReporter, Rectangle x_detectionArea)
    {
      var detectionArea = x_detectionArea;

      // 1. Initialisierung durchführen und Zeit messen
      var sw = Stopwatch.StartNew();
      m_regexDetectionService.InitializeRun (detectionArea);
      sw.Stop ();
      var initMs = sw.Elapsed.TotalMilliseconds;
      m_loggingService.Log ($"[DirectX] Initialization completed in {sw.Elapsed.TotalMilliseconds:F2} ms.");

      // 2. Kalibrierung durchführen und messen
      sw.Restart (); // Setzt die Uhr auf 0 und startet sie neu
      var detectionResult = m_regexDetectionService.Calibrate (detectionArea);
      sw.Stop ();
      var calibMs = sw.Elapsed.TotalMilliseconds;
      m_loggingService.Log ($"[EmguCV] Calibration completed in {sw.Elapsed.TotalMilliseconds:F2} ms. Result: {detectionResult}");

      // Kalibrierung überprüfen.
      if (detectionResult != DetectionResult.CalibrationSuccess)
      {
        // Kalibrierung fehlgeschlagen, Fehler-Overlay anzeigen.
        x_progressReporter.Report (new CraftDetectionProgress (detectionResult));
        return;
      }

      // Detektion starten.
      while (cancellationToken.IsCancellationRequested == false)
      {
        if (m_regexDetectionService.Detect (detectionArea) == DetectionResult.RegexDetectionSuccess)
        {
          x_progressReporter.Report (new CraftDetectionProgress (DetectionResult.RegexDetectionSuccess));
          break;
        }

        await Task.Delay (16, cancellationToken);
      }
    }

    private async void OnProgressChanged (CraftDetectionProgress x_progress)
    {
      await ShowOverlayAsync (x_progress.DetectionResult);
    }

    public async Task ShowOverlayAsync (DetectionResult x_detectionResult)
    {
      IsOverlayVisible = true;
      CurrentDetectionResult = x_detectionResult;
      IsOkayEnabled = false;

      await Task.Delay (100);

      // Sound abspielen, wenn aktiviert.
      if (CurrentDetectionResult == DetectionResult.RegexDetectionSuccess)
      {
        var currentSettings = m_settingsService.LoadSettings<UserSettings> ();
        if (currentSettings.SoundEnabled == true)
          m_soundPlayerService.PlaySound (currentSettings.SoundType, currentSettings.SoundVolume);

        // 700ms warten, bis der Button aktiviert wird, damit der Benutzer das Overlay nicht ausversehen sofort wieder schließt.
        await Task.Delay (700);
        IsOkayEnabled = true;
      }

      IsOkayEnabled = true;
    }

    private void OnSettings (Window x_window)
    {
      var currentSettings = m_settingsService.LoadSettings<SettingsWindowSettings> ();
      currentSettings.SettingsWindowTop = WindowTop;
      currentSettings.SettingsWindowLeft = WindowLeft + WindowWidth;
      m_settingsService.SaveSettings (currentSettings);

      // Dialog öffnen.
      var viewModel = m_dialogService.ShowDialog<SettingsViewModel, SettingsDialog> ();

      // GUI aktualisieren.
      OnPropertyChanged (nameof (CanSettings));
      OnPropertyChanged (nameof (CanStartStop));
      OnPropertyChanged (nameof (CanExit));
      OnPropertyChanged (nameof (CanMinimize));
      OnPropertyChanged (nameof (CanDrag));
    }

    private void OnOkay ()
    {
      IsOverlayVisible = false;
    }

    private void OnExit ()
    {
      var windowSettings = new MainWindowSettings ();
      windowSettings.MainWindowHeight = WindowHeight;
      windowSettings.MainWindowWidth = WindowWidth;
      windowSettings.MainWindowLeft = WindowLeft;
      windowSettings.MainWindowTop = WindowTop;

      m_settingsService.SaveSettings (windowSettings);

      m_applicationService.ShutdownApp ();
    }

    private void OnMinimize ()
    {
      m_applicationService.MinimizeApp ();
    }

    public void Dispose ()
    {
      if (m_cancellationTokenSource != null)
      {
        m_cancellationTokenSource.Cancel ();
        m_cancellationTokenSource.Dispose ();
        m_cancellationTokenSource = null;
      }

      m_loggingService.Log ("Shutdown of POE CraftHelper");
    }

    //private void DoDeveloperStressTest (Rectangle x_detectionArea)
    //{
    //  m_regexDetectionService.InitializeRun (x_detectionArea);

    //  bool success = true;
    //  int iterations = 300;
    //  double totalElapsedMs = 0;
    //  int validCaptures = 0; // Zähler für tatsächliche Messungen

    //  for (int i = 0; i < iterations; i++)
    //  {
    //    var iterationStopwatch = Stopwatch.StartNew();

    //    var result = m_regexDetectionService.StressTest(x_detectionArea);

    //    iterationStopwatch.Stop ();

    //    // Wir addieren die Zeit IMMER, damit wir sehen, wie lange die CPU arbeitet
    //    totalElapsedMs += iterationStopwatch.Elapsed.TotalMilliseconds;

    //    if (result == DetectionResult.None)
    //    {
    //      // Bei einem Timeout kurz warten, damit die CPU nicht 100% Last erzeugt
    //      Thread.Sleep (1);
    //      continue;
    //    }

    //    // Wenn wir hier sind, hatten wir eine erfolgreiche Erkennung (oder einen echten Fehler)
    //    validCaptures++;

    //    if (result != DetectionResult.CalibrationRegexDetected)
    //    {
    //      success = false;
    //      m_loggingService.Log ($"Stress test failed at iteration {i + 1} with result: {result}");
    //      break;
    //    }
    //  }

    //  // Durchschnitt berechnen (entweder auf alle Iterationen oder nur auf validCaptures)
    //  double averageMs = iterations > 0 ? totalElapsedMs / iterations : 0;

    //  m_loggingService.Log ($"Stress test completed: Success={success}, " +
    //                $"Total time={totalElapsedMs:F2} ms, " +
    //                $"Average per iteration={averageMs:F2} ms, " +
    //                $"Valid detections={validCaptures}/{iterations}");
    //}

  }
}
