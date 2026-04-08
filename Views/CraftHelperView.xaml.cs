using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using POECraftHelper.ViewModels;

namespace POECraftHelper.Views
{
  /// <summary>
  /// Interaction logic for CraftHelperView.xaml
  /// </summary>
  public partial class CraftHelperView : MetroWindow
  {
    public CraftHelperView (CraftHelperViewModel x_viewModel)
    {
      DataContext = x_viewModel;
      InitializeComponent ();
    }
  }
}
