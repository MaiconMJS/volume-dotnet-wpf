using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace Volume
{
    public partial class HomeView : MetroWindow
    {
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;

        private bool _isDragging = false;
        private System.Windows.Point _dragStartPoint;

        public HomeView()
        {
            InitializeComponent();
            SetupTrayIcon();
        }

        private void ControlCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            ControlCircle.CaptureMouse();
        }

        private void ControlCircle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ControlCircle.ReleaseMouseCapture();
        }

        private void ControlCircle_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(this);
                double deltaY = currentPosition.Y - _dragStartPoint.Y;

                double currentTop = Canvas.GetTop(ControlCircle);
                double newTop = currentTop + deltaY;

                newTop = Math.Max(20, Math.Min(260, newTop));

                int percent = (int)((260 - newTop) * 100 / (260 - 20));

                Percent.Text = $"{percent}%";

                Canvas.SetTop(ControlCircle, newTop);

                _dragStartPoint = currentPosition;
            }
        }

        private void SetupTrayIcon()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Abrir", null, (_, _) => ShowWindow());
            _trayMenu.Items.Add("Sair", null, (_, _) =>
            {
                _trayIcon?.Dispose();
                _trayIcon = null;
                Application.Current.Shutdown();
            });

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("Assets/appicon-black.ico"),
                Visible = true,
                Text = "Volume",
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ShowWindow();
                }
            };
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}