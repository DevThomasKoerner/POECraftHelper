using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
