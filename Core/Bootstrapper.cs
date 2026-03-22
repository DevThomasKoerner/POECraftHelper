using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using POECraftHelper.Services;

namespace POECraftHelper.Core
{
  public static class Bootstrapper
  {
    public static IServiceProvider Configure ()
    {
      var services = new ServiceCollection ();

      // Services
      services.AddSingleton<IScreenshotCaptureService, ScreenshotCaptureService> ();
      services.AddSingleton<ICraftDetectionService, CraftDetectionService> ();
      services.AddSingleton<ILoggingService, LoggingService> ();
      services.AddSingleton<IWindowService, WindowService> ();
      services.AddSingleton<ISoundPlayerService, SoundPlayerService> ();
      services.AddSingleton<ISettingsService, SettingsService> ();

      // ViewModels
      services.AddSingleton<ViewModels.CraftHelperViewModel> ();
      services.AddTransient<ViewModels.SettingsViewModel> ();
      services.AddTransient<ViewModels.OverlayViewModel> ();

      // Views
      services.AddSingleton<Views.CraftHelperView> ();
      services.AddTransient<Views.SettingsView> ();
      services.AddTransient<Views.OverlayView> ();

      return services.BuildServiceProvider ();
    }
  }
}
