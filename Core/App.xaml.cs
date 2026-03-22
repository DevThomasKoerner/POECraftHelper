using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace POECraftHelper.Core
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private IServiceProvider m_serviceProvider;

    protected override void OnStartup (StartupEventArgs x_eventArgs)
    {
      base.OnStartup (x_eventArgs);
      m_serviceProvider = Bootstrapper.Configure ();

      var mainView = m_serviceProvider.GetRequiredService<Views.CraftHelperView>();

      MainWindow = mainView;

      ShutdownMode = ShutdownMode.OnMainWindowClose;

      mainView.Show ();
    }

    protected override void OnExit (ExitEventArgs x_eventArgs)
    {
      if (m_serviceProvider is IDisposable disposable)
      {
        disposable.Dispose ();
      }

      base.OnExit (x_eventArgs);
    }
  }
}
