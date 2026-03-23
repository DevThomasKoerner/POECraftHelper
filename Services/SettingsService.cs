using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Interfaces;
using POECraftHelper.Models;
using POECraftHelper.Properties;

namespace POECraftHelper.Services
{
  public interface ISettingsService
  {
    T LoadSettings<T> () where T : ISettings, new();
    void SaveSettings<T> (T x_settings) where T : ISettings;
  }

  public class SettingsService : ISettingsService
  {
    private readonly ILoggingService m_loggingService;

    public SettingsService (ILoggingService x_loggingService)
    {
      m_loggingService = x_loggingService;
    }

    public T LoadSettings<T> () where T : ISettings, new()
    {
      var settings = new T();
      settings.Load ();
      return settings;
    }

    public void SaveSettings<T> (T x_settings) where T : ISettings
    {
      x_settings.Save ();
      Properties.Settings.Default.Save ();
    }
  }
}
