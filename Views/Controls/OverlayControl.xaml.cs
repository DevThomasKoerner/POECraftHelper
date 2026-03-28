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

namespace POECraftHelper.Controls
{
  /// <summary>
  /// Interaction logic for OverlayView.xaml
  /// </summary>
  public partial class OverlayControl : UserControl
  {
    public static readonly DependencyProperty OkayCommandProperty = DependencyProperty.Register (nameof (OkayCommand),
                                                                                                         typeof (ICommand),
                                                                                                         typeof (OverlayControl),
                                                                                                         new PropertyMetadata (null));

    public ICommand OkayCommand
    {
      get => (ICommand)GetValue (OkayCommandProperty);
      set => SetValue (OkayCommandProperty, value);
    }

    public static readonly DependencyProperty IsOkayEnabledProperty = DependencyProperty.Register (nameof (IsOkayEnabled),
                                                                                                   typeof (Boolean),
                                                                                                   typeof (OverlayControl),
                                                                                                   new PropertyMetadata (false));

    public Boolean IsOkayEnabled
    {
      get => (Boolean)GetValue (IsOkayEnabledProperty);
      set => SetValue (IsOkayEnabledProperty, value);
    }

    public OverlayControl ()
    {
      InitializeComponent ();
    }
  }
}
