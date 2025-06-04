using MahApps.Metro.Controls;
using NAudio.CoreAudioApi;
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
            Percent.Text = GetMasterSystem();
            SyncControlPositionToSystemVolume();
        }

        // Evento para definir volume do sistema
        private static void SetMasterVolume(string percentString)
        {
            int percent = int.Parse(percentString.Replace("%", ""));

            var enumerador = new MMDeviceEnumerator();
            var device = enumerador.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float volume = percent / 100f;
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        // Evento para obter o volume atual do sistema
        private static string GetMasterSystem()
        {
            var enumerador = new MMDeviceEnumerator();
            var device = enumerador.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float scalar = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            int percent = (int)(scalar * 100);
            return $"{percent}%";
        }

        // Evento para sincronizar a barra de volume no start do app
        private void SyncControlPositionToSystemVolume()
        {
            var enumerador = new MMDeviceEnumerator();
            var device = enumerador.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            double newTop = 260 - (volume * (260 - 20));

            Canvas.SetTop(ControlCircle, newTop);

            double height = 280 - newTop;
            Canvas.SetTop(LineBlue, newTop);
            LineBlue.Height = height;
        }

        // Mouse event click DOWN 
        private void ControlCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            ControlCircle.CaptureMouse();
        }

        // Mouse event click UP 
        private void ControlCircle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ControlCircle.ReleaseMouseCapture();
        }

        // Mouse event click MOVE
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

                SetMasterVolume(Percent.Text);

                Canvas.SetTop(ControlCircle, newTop);

                // Atualiza a linha azul de acordo com o nível de volume do sistema
                double height = 280 - newTop;
                Canvas.SetTop(LineBlue, newTop);
                LineBlue.Height = height;

                _dragStartPoint = currentPosition;
            }
        }

        // Configuração da bandeja do sistema
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

        // Exibir a janela principal quando o ícone da bandeja é clicado
        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        // Evento para fechar a janela, mas manter o ícone na bandeja do sistema
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}