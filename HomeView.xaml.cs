using MahApps.Metro.Controls;

using System.ComponentModel;
using System.Windows;
using Application = System.Windows.Application;

namespace Volume
{
    public partial class HomeView : MetroWindow
    {
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;

        public HomeView()
        {
            InitializeComponent();
            SetupTrayIcon();
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