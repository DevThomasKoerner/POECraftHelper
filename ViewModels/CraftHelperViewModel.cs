using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using POECraftHelper.Core;
using POECraftHelper.Models;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class CraftHelperViewModel : ObservableObject, IDisposable
  {
    #region Services

    private readonly ICraftDetectionService m_craftDetectionService;

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



    #endregion



    #region Commands

    public ICommand StartStopCommand { get; }
    public ICommand SettingsCommand { get; }

    #endregion

    #region CanProperties

    public Boolean CanStartStop => (m_windowService.IsSettingsOpen () == false);

    public Boolean CanSettings => (IsRunning == false) && (m_windowService.IsSettingsOpen () == false);

    #endregion


    public CraftHelperViewModel (ICraftDetectionService x_craftDetectionService, 
                                 IWindowService x_windowService, 
                                 ILoggingService x_loggingService,
                                 ISoundPlayerService x_soundPlayerService,
                                 ISettingsService x_settingsService)
    {
      m_craftDetectionService = x_craftDetectionService;
      m_windowService = x_windowService;
      m_loggingService = x_loggingService;
      m_soundPlayerService = x_soundPlayerService;
      m_settingsService = x_settingsService;

      m_windowService.SettingWindowClosed += OnSettingsClosed;

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

        m_currentSettings = m_settingsService.LoadSettings ();

        await Task.Run (() => StartAsync (m_cancellationTokenSource.Token, craftDetectionProgress));
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

    private async Task StartAsync (CancellationToken cancellationToken, IProgress<CraftDetectionProgress> x_progressReporter)
    {
      while (cancellationToken.IsCancellationRequested == false)
      {
        await Task.Delay (2000, cancellationToken);
        x_progressReporter.Report (new CraftDetectionProgress (true));
        break;
      }
    }

    private void OnProgressChanged (CraftDetectionProgress x_progress)
    {
      if (x_progress.RegexHit == true)
      {
        m_windowService.ShowOverlay ();

        m_soundPlayerService.PlaySound (m_currentSettings.SoundType);

        m_loggingService.Log ("Regex hit.");
      }
    }

    private void OnSettings (Window x_window)
    {
      if (m_windowService.IsSettingsOpen () == true)
        return;

      m_windowService.ShowSettings (x_window);

      // GUI aktualisieren.
      OnPropertyChanged (nameof (CanSettings));
      OnPropertyChanged (nameof (CanStartStop));
    }

    private void OnSettingsClosed (Object sender, EventArgs e)
    {
      // GUI aktualisieren.
      OnPropertyChanged (nameof (CanSettings));
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
