using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using POECraftHelper.Core;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class OverlayViewModel
  {
    #region Services 

    private readonly IWindowService m_windowService;

    #endregion

    #region Public Properties

    public event Action RequestClose;

    #endregion

    #region Commands

    public ICommand OkayCommand { get; }

    #endregion


    public OverlayViewModel (IWindowService x_windowService)
    {
      m_windowService = x_windowService;

      OkayCommand = new RelayCommand (OnOkay);
    }

    private void OnOkay ()
    {
      // Hier overlayfenster schließen.
      RequestClose?.Invoke ();
    }
  }
}
