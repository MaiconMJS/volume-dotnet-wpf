using System.Diagnostics;
using WebSocketSharp;

namespace Volume
{
    public class WebSocketReceiver
    {
        private static WebSocket? ws;

        public static void Start()
        {
            if (ws != null && ws.IsAlive) return;

            ws = new WebSocket("ws://localhost:3001");

            ws.OnOpen += Ws_OnOpen;
            ws.OnMessage += Ws_OnMessage;
            ws.OnError += Ws_OnError;
            ws.OnClose += Ws_OnClose;

            ws.Connect();
        }

        private static void Ws_OnOpen(object? sender, EventArgs e)
        {
            Debug.WriteLine("✅ Conectado ao servidor WebSocket.");

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (HomeView.Instance != null)
                {
                    HomeView.Instance.WebSocketIndicator.Fill = System.Windows.Media.Brushes.Green;
                }
            });
        }

        private static void Ws_OnMessage(object? sender, MessageEventArgs e)
        {
            string message = e.Data;

            Debug.WriteLine($"📨 Recebido via WebSocket: {message}");

            HomeView.SetMasterVolume(message);

            if (int.TryParse(message, out int percent))
            {
                HomeView.Instance?.UpdateVolumeUIFromPercentage(percent);
            }
        }

        private static void Ws_OnError(object? sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"❌ Erro WebSocket: {e.Message}");
        }

        private static void Ws_OnClose(object? sender, CloseEventArgs e)
        {
            Debug.WriteLine($"🔌 WebSocket desconectado: {e.Reason}");

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (HomeView.Instance != null)
                {
                    HomeView.Instance.WebSocketIndicator.Fill = System.Windows.Media.Brushes.Red;
                }
            });
        }
    }
}
