using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Interfaces
{
  public interface IDialogAware
  {
    event Action<Boolean> RequestClose;
    Boolean DialogResult { get; set; }
  }
}
