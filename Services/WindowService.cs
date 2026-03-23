using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media.Media3D;
using Microsoft.Extensions.DependencyInjection;
using POECraftHelper.Models;
using POECraftHelper.ViewModels;
using POECraftHelper.Views;

namespace POECraftHelper.Services
{

  public interface IWindowService
  {
    void Initialize ();

    void ShowSettings (Window x_owner);

    void CloseSettings ();

    Boolean IsSettingsOpen ();

    event EventHandler SettingWindowClosed;


    void ShowOverlay (Rect x_bounds);


  }

  public class WindowService : IWindowService
  {
    private readonly IServiceProvider m_serviceProvider;

    private readonly ISettingsService m_settingsService;

    private SettingsView m_settingsWindow;

    private OverlayView m_overlayView;

    public event EventHandler SettingWindowClosed;

    public WindowService (IServiceProvider x_serviceProvider, ISettingsService x_settingsService)
    {
      m_serviceProvider = x_serviceProvider;
      m_settingsService = x_settingsService;
    }

    public void Initialize ()
    {
      if (m_overlayView != null)
        return;

      var overlayViewModel = m_serviceProvider.GetRequiredService<OverlayViewModel> ();
      var overlayView = m_serviceProvider.GetRequiredService<OverlayView> ();

      overlayView.DataContext = overlayViewModel;
      overlayView.Visibility = Visibility.Hidden;

      overlayViewModel.RequestClose += () =>
      {
        m_overlayView.Hide ();
      };

      m_overlayView = overlayView;

      m_overlayView.Show ();
      m_overlayView.Hide ();
    }

    public void ShowSettings (Window x_owner)
    {
      if (m_settingsWindow != null)
        return;

      var windowSettings = m_settingsService.LoadSettings<WindowSettings> ();

      var view = m_serviceProvider.GetRequiredService<SettingsView>();
      var viewModel = m_serviceProvider.GetRequiredService<SettingsViewModel>();

      view.DataContext = viewModel;
      view.Owner = x_owner;

      // Position rechts daneben
      view.WindowStartupLocation = WindowStartupLocation.Manual;
      viewModel.WindowLeft = x_owner.Left + x_owner.Width;
      viewModel.WindowTop = x_owner.Top;
      viewModel.WindowHeight = windowSettings.SettingsWindowHeight;
      viewModel.WindowWidth = windowSettings.SettingsWindowWidth;

      // Referenz speichern, damit das Fenster nicht erneut geöffnet wird, wenn es bereits offen ist.
      m_settingsWindow = view;

      view.Show ();
    }

    public void ShowOverlay (Rect x_bounds)
    {
      if (m_overlayView == null)
        return;

      m_overlayView.Left = x_bounds.Left;
      m_overlayView.Top = x_bounds.Top;
      m_overlayView.Width = x_bounds.Width;
      m_overlayView.Height = x_bounds.Height;

      if (m_overlayView.IsVisible == false)
        m_overlayView.Show ();

      m_overlayView.Activate ();
    }

    public Boolean IsSettingsOpen ()
    {
      return m_settingsWindow != null;
    }

    public void CloseSettings ()
    {
      if (m_settingsWindow != null)
      {
        m_settingsWindow.Close ();
        m_settingsWindow = null;
      }

      SettingWindowClosed?.Invoke (this, EventArgs.Empty);
    }
  }
}
