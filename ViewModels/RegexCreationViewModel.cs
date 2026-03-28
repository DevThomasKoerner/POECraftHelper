using System.Windows.Input;
using POECraftHelper.Core;

namespace POECraftHelper.ViewModels
{
  public class RegexCreationViewModel : DialogViewModelBase
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
        OnPropertyChanged (nameof (CanAdd));
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
        OnPropertyChanged (nameof (CanAdd));
      }
    }

    #endregion

    #region Can Properties

    public Boolean CanAdd => (String.IsNullOrEmpty (RegexTitle) == false) && (String.IsNullOrEmpty (RegexPattern) == false);

    #endregion

    #region Commands

    public ICommand AddCommand { get; }

    public ICommand CancelCommand { get; }

    #endregion

    public RegexCreationViewModel ()
    {
      AddCommand = new RelayCommand (OnAdd);
      CancelCommand = new RelayCommand (OnCancel);
    }

    private void OnAdd ()
    {
      DialogResult = true;
      Close (true);
    }

    private void OnCancel ()
    {
      DialogResult = false;
      Close (false);
    }
  }
}
