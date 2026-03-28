using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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

    private void Border_SizeChanged (object sender, SizeChangedEventArgs e)
    {
      var border = (Border)sender;

      // Border-Dicke
      var thickness = border.BorderThickness;

      // MainWindow
      var window = Window.GetWindow(border);
      if (window == null)
        return;

      // Position des Borders relativ zum Window
      var topLeftRelative = border.TranslatePoint(new System.Windows.Point(0, 0), window);

      // DPI-Skalierung
      var dpi = VisualTreeHelper.GetDpi(window);

      // Rectangle in Bildschirm-Pixeln
      var rect = new Rectangle(
        (int)((window.Left + topLeftRelative.X + thickness.Left + 1) * dpi.DpiScaleX),
        (int)((window.Top + topLeftRelative.Y + thickness.Top) * dpi.DpiScaleY),
        (int)(Math.Max(0, (border.ActualWidth - thickness.Left - thickness.Right - 1) * dpi.DpiScaleX)),
        (int)(Math.Max(0, (border.ActualHeight - thickness.Top - thickness.Bottom) * dpi.DpiScaleY))
    );

      // Setze ins ViewModel
      if (DataContext is CraftHelperViewModel vm)
        vm.SetInnerBorderRect (rect);
    }
  }
}
