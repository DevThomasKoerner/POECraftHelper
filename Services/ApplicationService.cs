using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media.Media3D;
using Microsoft.Extensions.DependencyInjection;
using POECraftHelper.Dialogs;
using POECraftHelper.Models;
using POECraftHelper.ViewModels;
using POECraftHelper.Views;

namespace POECraftHelper.Services
{

  public interface IApplicationService
  {
    void ShutdownApp ();

    void MinimizeApp ();
  }

  public class ApplicationService : IApplicationService
  {
    public ApplicationService ()
    {}

    public void ShutdownApp ()
    {
      Application.Current.Shutdown ();
    }

    public void MinimizeApp ()
    {
      var window = Application.Current.MainWindow;
      if (window != null)
      {
        window.WindowState = WindowState.Minimized;
      }
    }
  }
}
