using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace POECraftHelper.Controls
{
  [StructLayout (LayoutKind.Sequential)]
  struct POINT
  {
    public int X;
    public int Y;
  }

  /// <summary>
  /// Interaction logic for WindowResizeControl.xaml
  /// </summary>
  public partial class WindowResizeControl : UserControl
  {
    [DllImport ("user32.dll")]
    [return: MarshalAs (UnmanagedType.Bool)]
    static extern bool GetCursorPos (out POECraftHelper.Controls.POINT lpPoint);

    private POINT m_startMousePos;
    private Double m_startWidth;
    private Double m_startHeight;
    private Double m_startLeft;
    private const Double m_sensitivity = 0.5;

    public WindowResizeControl ()
    {
      InitializeComponent ();

      UpdateIconKind ();
    }

    public static readonly DependencyProperty IsVerticalProperty = DependencyProperty.Register (nameof (IsVertical), 
                                                                                                typeof (Boolean), 
                                                                                                typeof (WindowResizeControl), 
                                                                                                new PropertyMetadata (true, OnDirectionChanged));

    public Boolean IsVertical
    {
      get => (Boolean)GetValue (IsVerticalProperty);
      set => SetValue (IsVerticalProperty, value);
    }

    private static void OnDirectionChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is WindowResizeControl control)
      {
        Boolean isVertical = (Boolean)e.NewValue;
        control._resizeIcon.Kind = isVertical ? MahApps.Metro.IconPacks.PackIconMaterialKind.ArrowUpDown : MahApps.Metro.IconPacks.PackIconMaterialKind.ArrowLeftRight;
      }
    }

    public static readonly DependencyProperty IsResizeEnabledProperty = DependencyProperty.Register (nameof (IsResizeEnabled),
                                                                                                     typeof (Boolean),
                                                                                                     typeof (WindowResizeControl),
                                                                                                     new PropertyMetadata (true));

    public static readonly DependencyProperty TargetElementProperty =
    DependencyProperty.Register(
        nameof(TargetElement),
        typeof(FrameworkElement),
        typeof(WindowResizeControl),
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
        typeof(WindowResizeControl),
        new FrameworkPropertyMetadata(default(Rectangle),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public Rectangle Bounds
    {
      get => (Rectangle)GetValue (BoundsProperty);
      set => SetValue (BoundsProperty, value);
    }

    public Boolean IsResizeEnabled
    {
      get => (Boolean)GetValue (IsResizeEnabledProperty);
      set => SetValue (IsResizeEnabledProperty, value);
    }

    private void UpdateIconKind ()
    {
      if (_resizeIcon == null)
        return;

      _resizeIcon.Kind = IsVertical ? MahApps.Metro.IconPacks.PackIconMaterialKind.ArrowUpDown : MahApps.Metro.IconPacks.PackIconMaterialKind.ArrowLeftRight;
    }

    private void Ellipse_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
    {
      if (IsResizeEnabled == false)
        return;

      if (Window.GetWindow (this) is Window window)
      {
        GetCursorPos (out m_startMousePos);
        m_startWidth = window.Width;
        m_startHeight = window.Height;
        m_startLeft = window.Left;


        (sender as UIElement)?.CaptureMouse ();
        (sender as UIElement).MouseMove += Ellipse_MouseMove;
        (sender as UIElement).MouseLeftButtonUp += Ellipse_MouseLeftButtonUp;
      }
    }

    private void Ellipse_MouseMove (object sender, MouseEventArgs e)
    {
      if (Window.GetWindow (this) is Window window && e.LeftButton == MouseButtonState.Pressed)
      {
        GetCursorPos (out POINT currentPos);

        if (IsVertical)
        {
          double deltaY = (currentPos.Y - m_startMousePos.Y) * m_sensitivity;
          window.Height = Math.Max (window.MinHeight > 0 ? window.MinHeight : 50, m_startHeight + deltaY);
        }
        else
        {
          double deltaX = (currentPos.X - m_startMousePos.X) * m_sensitivity;

          // Linksseitiges Resize
          double newWidth = Math.Max(window.MinWidth > 0 ? window.MinWidth : 50, m_startWidth - deltaX);
          double newLeft = m_startLeft + deltaX;

          window.Width = newWidth;
          window.Left = newLeft;
        }
      }
    }

    private void Ellipse_MouseLeftButtonUp (object sender, MouseButtonEventArgs e)
    {
      (sender as UIElement)?.ReleaseMouseCapture ();
      (sender as UIElement).MouseMove -= Ellipse_MouseMove;
      (sender as UIElement).MouseLeftButtonUp -= Ellipse_MouseLeftButtonUp;

      if (TargetElement is Border border)
        Application.Current.Dispatcher.InvokeAsync (UpdateTargetBounds, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void Ellipse_MouseEnter (object sender, MouseEventArgs e)
    {
      if (IsResizeEnabled == false)
        return;

      Mouse.OverrideCursor = IsVertical ? Cursors.SizeNS : Cursors.SizeWE;
    }

    private void Ellipse_MouseLeave (object sender, MouseEventArgs e)
    {
      if (IsResizeEnabled == false)
        return;

      Mouse.OverrideCursor = null;
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
  }
}
