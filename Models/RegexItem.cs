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
    private String m_regexName;
    public String RegexName
    {
      get => m_regexName;
      set 
      { 
        m_regexName = value; 
        OnPropertyChanged (nameof (RegexName)); 
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

    public RegexItem (String x_regexName, String x_regexPattern)
    {
      RegexName = x_regexName;
      RegexPattern = x_regexPattern;
    }
  }
}
