using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Core;

namespace POECraftHelper.Models
{
  public class RegexItem : ObservableObject
  {
    #region Binding Properties

    private String m_regexTitle;
    public String RegexTitle
    {
      get => m_regexTitle;
      set 
      {
        m_regexTitle = value; 
        OnPropertyChanged (nameof (RegexTitle)); 
      }
    }

    private String m_regexPattern;
    public String RegexPattern
    {
      get => m_regexPattern;
      set
      {
        m_regexPattern = value;
        OnPropertyChanged (nameof (RegexPattern));
      }
    }

    private bool m_isCopiedToClipboard;
    public bool IsCopiedToClipboard
    {
      get => m_isCopiedToClipboard;
      set
      {
        m_isCopiedToClipboard = value;
        OnPropertyChanged (nameof (IsCopiedToClipboard));
      }
    }

    #endregion

    public RegexItem (String x_regexTitle, String x_regexPattern)
    {
      RegexTitle = x_regexTitle;
      RegexPattern = x_regexPattern;
    }
  }
}
