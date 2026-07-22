using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
      services.AddSingleton<IScreenshotCaptureService, ScreenshotCaptureDirectXService> ();
      //services.AddSingleton<IScreenshotCaptureService, ScreenshotCaptureService> ();
      services.AddSingleton<IRegexDetectionService, RegexDetectionService> ();
      services.AddSingleton<ILoggingService, LoggingService> ();
      services.AddSingleton<IApplicationService, ApplicationService> ();
      services.AddSingleton<ISoundPlayerService, SoundPlayerService> ();
      services.AddSingleton<ISettingsService, SettingsService> ();
      services.AddSingleton<IDialogService, DialogService> ();
      services.AddSingleton<IUpdateService, UpdateService> ();
      services.AddHttpClient<IUpdateService, UpdateService> (client =>
      {
        client.DefaultRequestHeaders.UserAgent.ParseAdd ("POECraftHelper/1.0");
        client.Timeout = TimeSpan.FromSeconds (10);
      });

      // Hauptansicht
      services.AddSingleton<ViewModels.CraftHelperViewModel> ();
      services.AddSingleton<Views.CraftHelperView> ();

      // Settings-Dialog
      services.AddTransient<ViewModels.SettingsViewModel> ();
      services.AddTransient<Dialogs.SettingsDialog> ();

      // RegexCreation-Dialog
      services.AddTransient<ViewModels.RegexCreationViewModel> ();
      services.AddTransient<Dialogs.RegexCreationDialog> ();

      return services.BuildServiceProvider ();
    }
  }
}
