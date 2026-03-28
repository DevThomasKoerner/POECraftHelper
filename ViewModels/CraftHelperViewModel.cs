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

    //private readonly IOverlayService m_overlayService;

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

    public Rectangle InnerBorderRect { get; private set; }

    public void SetInnerBorderRect (Rectangle rect)
    {
      InnerBorderRect = rect;
      // Optional: PropertyChanged, wenn du es gebunden hast
      OnPropertyChanged (nameof (InnerBorderRect));
    }

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
      IsOverlayVisible = false;

      var windowSettings = m_settingsService.LoadSettings<MainWindowSettings> ();
      WindowTop = windowSettings.MainWindowTop;
      WindowLeft = windowSettings.MainWindowLeft;
      WindowHeight = windowSettings.MainWindowHeight;
      WindowWidth = windowSettings.MainWindowWidth;
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

        // Detektionsbereich ermitteln.
        var detectionArea = GetDetectionArea ();

        // Detektionsprozess starten.
        await Task.Run (() => StartDetectionAsync (m_cancellationTokenSource.Token, craftDetectionProgress, detectionArea));
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
      await Task.Delay (1000);
      while (cancellationToken.IsCancellationRequested == false)
      {
        if (m_regexDetectionService.Detect (detectionArea) == true)
        {
          x_progressReporter.Report (new CraftDetectionProgress (true));
          break;
        }
      }
    }

    private Rectangle GetDetectionArea ()
    {
      return InnerBorderRect;
    }

    private async void OnProgressChanged (CraftDetectionProgress x_progress)
    {
      if (x_progress.RegexHit == true)
      {
        m_loggingService.Log ("Regex hit.");

        await ShowOverlayAsync ();
      }
    }

    public async Task ShowOverlayAsync ()
    {
      IsOverlayVisible = true;
      IsOkayEnabled = false;

      // Sound abspielen, wenn aktiviert.
      var currentSettings = m_settingsService.LoadSettings<UserSettings> ();
      if (currentSettings.SoundEnabled == true)
        m_soundPlayerService.PlaySound (currentSettings.SoundType, currentSettings.SoundVolume);

      // 1 Sekunde warten, bis der Button aktiviert wird, damit der Benutzer das Overlay nicht ausversehen sofort wieder schließt.
      await Task.Delay (1000);
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
    }
  }
}
