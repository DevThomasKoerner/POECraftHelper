using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Microsoft.Extensions.DependencyInjection;
using POECraftHelper.ViewModels;
using POECraftHelper.Views;

namespace POECraftHelper.Services
{

  public interface IWindowService
  {
    void ShowSettings (Window owner);

    void ShowOverlay ();

    void CloseOverlayWindow (OverlayViewModel viewModel);

    Boolean IsSettingsOpen ();

    event EventHandler SettingWindowClosed;
  }

  public class WindowService : IWindowService
  {
    private readonly IServiceProvider m_serviceProvider;

    private SettingsView m_settingsWindow;

    public event EventHandler SettingWindowClosed;

    public WindowService (IServiceProvider x_serviceProvider)
    {
      m_serviceProvider = x_serviceProvider;
    }

    public void ShowSettings (Window owner)
    {
      if (m_settingsWindow != null)
        return;

      var view = m_serviceProvider.GetRequiredService<SettingsView>();
      var viewModel = m_serviceProvider.GetRequiredService<SettingsViewModel>();

      view.DataContext = viewModel;
      view.Owner = owner;

      // Position rechts daneben
      view.WindowStartupLocation = WindowStartupLocation.Manual;
      view.Left = owner.Left + owner.Width;
      view.Top = owner.Top;

      // Referenz speichern, damit das Fenster nicht erneut geöffnet wird, wenn es bereits offen ist.
      m_settingsWindow = view;

      view.Closed += OnSettingsClosed;

      view.Show ();
    }

    public void ShowOverlay ()
    {
      var viewModel = m_serviceProvider.GetRequiredService<OverlayViewModel> ();
      var view = m_serviceProvider.GetRequiredService<OverlayView> ();

      view.DataContext = viewModel;

      viewModel.RequestClose += () => view.Close ();

      view.Show ();
    }

    public void CloseOverlayWindow (OverlayViewModel viewModel)
    {
      // Alle offenen Fenster durchgehen und dasjenige schließen, das den übergebenen ViewModel als DataContext hat.
      foreach (Window window in Application.Current.Windows)
      {
        if (window.DataContext == viewModel)
        {
          window.Close ();
          break;
        }
      }
    }

    public Boolean IsSettingsOpen ()
    {
      return m_settingsWindow != null;
    }

    private void OnSettingsClosed (object sender, EventArgs e)
    {
      if (m_settingsWindow != null)
      {
        m_settingsWindow.Closed -= OnSettingsClosed;
        m_settingsWindow = null;
      }

      SettingWindowClosed?.Invoke (this, EventArgs.Empty);
    }
  }
}
