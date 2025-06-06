using MahApps.Metro.Controls;
using NAudio.CoreAudioApi;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application = System.Windows.Application;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Volume
{
    public partial class HomeView : MetroWindow
    {
        public static HomeView? Instance { get; private set; }
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;

        private double _startControlTop;

        private bool _isDragging = false;
        private System.Windows.Point _dragStartPoint;

        public HomeView()
        {
            InitializeComponent();
            Instance = this;
            SetupTrayIcon();
            Percent.Text = GetMasterSystem();
            SyncControlPositionToSystemVolume();
            // Executa o websocket em segundo plano sem interferir na thread principal da UI
            Task.Run(() => WebSocketReceiver.Start());
        }

        // Evento para definir volume do sistema
        public static void SetMasterVolume(string percentString)
        {
            int percent = int.Parse(percentString.Replace("%", ""));

            var enumerador = new MMDeviceEnumerator();
            var device = enumerador.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float volume = percent / 100f;
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;

            double newTop = 260 - (volume * (260 - 20));
            Instance!._startControlTop = newTop;

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

        // Animação de scala de texto para porcentagem de volume
        private void AnimateTextScale()
        {
            var scaleTransform = (ScaleTransform)Percent.RenderTransform;

            var scaleUp = new DoubleAnimation
            {
                To = 1.2,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
        }

        // Animação da barra de volume e linha azul
        private void AnimateVolumeUI(double targetTop)
        {
            double currentTop = Canvas.GetTop(ControlCircle);

            var animationDuration = new Duration(TimeSpan.FromMilliseconds(500));

            // Anima o Top do ControlCircle
            var topAnimation = new DoubleAnimation
            {
                From = currentTop,
                To = targetTop,
                Duration = animationDuration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            ControlCircle.BeginAnimation(Canvas.TopProperty, topAnimation);

            // Anima o Top da linha azul
            LineBlue.BeginAnimation(Canvas.TopProperty, new DoubleAnimation
            {
                From = Canvas.GetTop(LineBlue),
                To = targetTop,
                Duration = animationDuration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });

            // Anima a altura da linha azul
            double targetHeight = 280 - targetTop;
            LineBlue.BeginAnimation(HeightProperty, new DoubleAnimation
            {
                From = LineBlue.Height,
                To = targetHeight,
                Duration = animationDuration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
        }

        // Evento para atualizar a barra de volume quando controlado pela rede
        public void UpdateVolumeUIFromPercentage(int percent)
        {
            if (percent < 0 || percent > 100) return;

            Dispatcher.Invoke(() =>
            {
                Percent.Text = $"{percent}%";

                double newTop = 260 - (percent / 100.0 * (260 - 20));
                AnimateTextScale();
                AnimateVolumeUI(newTop);
            });
        }

        // Mouse event click DOWN 
        private void ControlCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _startControlTop = Canvas.GetTop(ControlCircle);

            ControlCircle.CaptureMouse();

            // ❌ Interrompe animações pendentes
            ControlCircle.BeginAnimation(Canvas.TopProperty, null);
            LineBlue.BeginAnimation(Canvas.TopProperty, null);
            LineBlue.BeginAnimation(HeightProperty, null);
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

                double newTop = _startControlTop + deltaY;

                newTop = Math.Max(20, Math.Min(260, newTop));

                int percent = (int)((260 - newTop) * 100 / (260 - 20));

                Percent.Text = $"{percent}%";

                SetMasterVolume(Percent.Text);

                Canvas.SetTop(ControlCircle, newTop);

                // Atualiza a linha azul de acordo com o nível de volume do sistema
                double height = 280 - newTop;
                Canvas.SetTop(LineBlue, newTop);
                LineBlue.Height = height;

                _startControlTop = newTop;
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