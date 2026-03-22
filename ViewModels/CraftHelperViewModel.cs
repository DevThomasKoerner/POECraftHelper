using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using POECraftHelper.Core;
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

    public Boolean CanStartStop => true;

    public Boolean CanSettings => (IsRunning == false) && (m_windowService.IsOpen () == false);

    #endregion


    public CraftHelperViewModel (ICraftDetectionService x_craftDetectionService, 
      IWindowService x_windowService, 
      ILoggingService x_loggingService,
      ISoundPlayerService x_soundPlayerService)
    {
      m_craftDetectionService = x_craftDetectionService;
      m_windowService = x_windowService;
      m_loggingService = x_loggingService;
      m_soundPlayerService = x_soundPlayerService;

      m_windowService.WindowClosed += OnSettingsClosed;

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

        await Task.Run (() => StartAsync (m_cancellationTokenSource.Token));
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

    private async Task StartAsync (CancellationToken cancellationToken)
    {
      while (cancellationToken.IsCancellationRequested == false)
      {
        await Task.Delay (5000, cancellationToken);

      }
    }

    private void OnSettings (Window x_window)
    {
      if (m_windowService.IsOpen () == true)
        return;

      m_windowService.ShowSettings (x_window);

      // GUI aktualisieren.
      OnPropertyChanged (nameof (CanSettings));
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
