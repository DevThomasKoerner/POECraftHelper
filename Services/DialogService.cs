using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using POECraftHelper.Interfaces;

namespace POECraftHelper.Services
{
  public interface IDialogService
  {
    TViewModel ShowDialog<TViewModel, TView> () where TViewModel : IDialogAware where TView : MetroWindow;
  }

  public class DialogService : IDialogService
  {
    private readonly IServiceProvider m_serviceProvider;
    public DialogService (IServiceProvider x_serviceProvider)
    {
      m_serviceProvider = x_serviceProvider ?? throw new ArgumentNullException (nameof (x_serviceProvider));
    }

    public TViewModel ShowDialog<TViewModel, TView> () where TViewModel : IDialogAware where TView : MetroWindow
    {
      // ViewModel vom Container
      var viewModel = m_serviceProvider.GetRequiredService<TViewModel>();
      var view = m_serviceProvider.GetRequiredService<TView> ();

      view.DataContext = viewModel;

      // ViewModel benachrichtigt die View, dass sie geschlossen werden soll
      viewModel.RequestClose += (result) =>
      {
        view.DialogResult = result;
        view.Close ();
      };

      view.ShowDialog ();

      return viewModel;
    }
  }
}
