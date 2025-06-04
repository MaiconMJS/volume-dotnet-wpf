using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Volume
{
    public class UdpReceiver
    {
        private static UdpClient? udpClient;

        public static void Start()
        {
            udpClient = new UdpClient(3001);
            udpClient.BeginReceive(ReceiveCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint remoteEP = new(IPAddress.Any, 0);
            byte[] data = udpClient!.EndReceive(ar, ref remoteEP!);
            string message = Encoding.UTF8.GetString(data);

            // Printa o volume recebido no console para debug
            Debug.WriteLine($"Recebido no WPF: {message}");
            // Passa o volume para o driver de som
            HomeView.SetMasterVolume(message);
            // Converte o a string para inteiro e atualiza a barra de volume
            if (int.TryParse(message, out int percent))
            {
                HomeView.Instance?.UpdateVolumeUIFromPercentage(percent);
            }
            // Continua ouvindo novos pacotes UDP
            udpClient.BeginReceive(ReceiveCallback, null);
        }
    }
}
