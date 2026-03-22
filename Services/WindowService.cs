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
    Boolean IsOpen ();

    event EventHandler WindowClosed;
  }

  public class WindowService : IWindowService
  {
    private readonly IServiceProvider m_serviceProvider;

    private SettingsView m_settingsWindow;

    public event EventHandler WindowClosed;

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



    public Boolean IsOpen ()
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

      WindowClosed?.Invoke (this, EventArgs.Empty);
    }
  }
}
