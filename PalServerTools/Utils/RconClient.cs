namespace PalServerTools.Utils
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class RconClient
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private byte[] receiveBuffer;

        public async Task ConnectAsync(string serverIp, int port, string password)
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(serverIp, port);
            networkStream = tcpClient.GetStream();
            receiveBuffer = new byte[4096];

            // 发送RCON认证请求
            await SendPacketAsync(0, RconPacketType.Auth, password);

            // 接收RCON认证响应
            var packet = await ReceivePacketAsync();

            if (packet.type == RconPacketType.AuthResponse)
            {
                Console.WriteLine("Authentication successful.");
            }
            else
            {
                Console.WriteLine("Authentication failed.");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (networkStream != null)
            {
                networkStream.Close();
                networkStream = null;
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
        }

        public async Task<string> SendCommandAsync(string command)
        {
            await SendPacketAsync(1, RconPacketType.Execute, command);
            var packet = await ReceivePacketAsync();
            string response = packet.payload;

            return response;
        }

        private async Task SendPacketAsync(int requestId, RconPacketType type, string payload)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            byte[] packet = new byte[14 + payloadBytes.Length];

            // Packet Size (little-endian)
            packet[0] = (byte)(packet.Length & 0xFF);
            packet[1] = (byte)(packet.Length >> 8 & 0xFF);
            packet[2] = (byte)(packet.Length >> 16 & 0xFF);
            packet[3] = (byte)(packet.Length >> 24 & 0xFF);

            // Request ID (little-endian)
            packet[4] = (byte)(requestId & 0xFF);
            packet[5] = (byte)(requestId >> 8 & 0xFF);
            packet[6] = (byte)(requestId >> 16 & 0xFF);
            packet[7] = (byte)(requestId >> 24 & 0xFF);

            // Type (little-endian)
            packet[8] = (byte)((int)type & 0xFF);
            packet[9] = (byte)((int)type >> 8 & 0xFF);
            packet[10] = (byte)((int)type >> 16 & 0xFF);
            packet[11] = (byte)((int)type >> 24 & 0xFF);

            // Payload
            Array.Copy(payloadBytes, 0, packet, 12, payloadBytes.Length);

            // Null terminator
            packet[12 + payloadBytes.Length] = 0;
            packet[13 + payloadBytes.Length] = 0;

            await networkStream.WriteAsync(packet, 0, packet.Length);
        }

        private async Task<(int requestId, RconPacketType type, string payload)> ReceivePacketAsync()
        {
            int packetSize = await networkStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

            // Read Request ID (little-endian)
            int requestId = BitConverter.ToInt32(receiveBuffer, 4);

            // Read Type (little-endian)
            RconPacketType type = (RconPacketType)BitConverter.ToInt32(receiveBuffer, 8);

            // Read Payload (UTF-8 encoded)
            string payload = Encoding.UTF8.GetString(receiveBuffer, 12, packetSize - 14);

            return (requestId, type, payload);
        }
    }

    public enum RconPacketType
    {
        Auth = 3,
        AuthResponse = 2,
        Execute = 2,
        Response = 0
    }
}
