using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

    public static readonly DependencyProperty TargetElementProperty =
    DependencyProperty.Register(
        nameof(TargetElement),
        typeof(FrameworkElement),
        typeof(WindowDragControl),
        new PropertyMetadata(null));

    public FrameworkElement TargetElement
    {
      get => (FrameworkElement)GetValue (TargetElementProperty);
      set => SetValue (TargetElementProperty, value);
    }


    public static readonly DependencyProperty BoundsProperty =
    DependencyProperty.Register(
        nameof(Bounds),
        typeof(Rectangle),
        typeof(WindowDragControl),
        new FrameworkPropertyMetadata(default(Rectangle),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public Rectangle Bounds
    {
      get => (Rectangle)GetValue (BoundsProperty);
      set => SetValue (BoundsProperty, value);
    }

    public WindowDragControl ()
    {
      InitializeComponent ();

      Loaded += (s, e) =>
      {
        LayoutUpdated += OnLayoutUpdated;
      };
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

      if (TargetElement is Border border)
        Application.Current.Dispatcher.InvokeAsync (UpdateTargetBounds, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void UpdateTargetBounds ()
    {
      if (TargetElement is not Border border)
        return;

      var window = Window.GetWindow (border);
      if (window == null)
        return;

      var thickness = border.BorderThickness;

      // Position relativ zum Window
      var topLeftRelative = border.TranslatePoint (new System.Windows.Point (0, 0), window);

      var dpi = VisualTreeHelper.GetDpi (window);

      var rect = new Rectangle ((int)((window.Left + topLeftRelative.X + thickness.Left + 1) * dpi.DpiScaleX),
                                (int)((window.Top + topLeftRelative.Y + thickness.Top) * dpi.DpiScaleY),
                                (int)(Math.Max(0, (border.ActualWidth - thickness.Left - thickness.Right) * dpi.DpiScaleX)),
                                (int)(Math.Max(0, (border.ActualHeight - thickness.Top - thickness.Bottom) * dpi.DpiScaleY)));

      if (Bounds != rect)
        Bounds = rect;
    }

    private void OnLayoutUpdated (object sender, EventArgs e)
    {
      LayoutUpdated -= OnLayoutUpdated;
      UpdateTargetBounds ();
    }
  }
}
