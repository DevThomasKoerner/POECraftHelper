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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POECraftHelper.Controls
{
  /// <summary>
  /// Interaction logic for WindowDragControl.xaml
  /// </summary>
  public partial class WindowDragControl : UserControl
  {
    public static readonly DependencyProperty IsDragEnabledProperty = DependencyProperty.Register (nameof (IsDragEnabled),
                                                                                                   typeof (Boolean),
                                                                                                   typeof (WindowDragControl),
                                                                                                   new PropertyMetadata (true));

    public Boolean IsDragEnabled
    {
      get => (Boolean)GetValue (IsDragEnabledProperty);
      set => SetValue (IsDragEnabledProperty, value);
    }

    public WindowDragControl ()
    {
      InitializeComponent ();
    }

    private void Ellipse_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
    {
      if (IsDragEnabled == false)
        return;

      // Holt das übergeordnete Fenster
      var window = Window.GetWindow (this);

      if (window != null)
      {
        try
        {
          // Startet das Fenster-Draggen
          window.DragMove ();
        }
        catch (InvalidOperationException)
        {
          // Kann passieren, wenn Linksklick während DragMove fehlschlägt
        }
      }
    }

    private void Ellipse_MouseEnter (object sender, MouseEventArgs e)
    {
      if (IsDragEnabled == false)
        return;

      Mouse.OverrideCursor = Cursors.SizeAll;
    }

    private void Ellipse_MouseLeave (object sender, MouseEventArgs e)
    {
      if (IsDragEnabled == false)
        return;

      Mouse.OverrideCursor = null;
    }
  }
}
