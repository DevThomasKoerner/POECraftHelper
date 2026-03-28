using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Core;
using POECraftHelper.Interfaces;

namespace POECraftHelper.ViewModels
{
  public class DialogViewModelBase : ObservableObject, IDialogAware
  {
    public event Action<Boolean> RequestClose;

    public Boolean DialogResult { get; set; }

    protected void Close (Boolean result)
    {
      DialogResult = result;
      RequestClose?.Invoke (result);
    }
  }
}
