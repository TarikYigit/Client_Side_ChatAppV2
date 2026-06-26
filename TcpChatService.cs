namespace Client_Side_ChatApp.Core
{
    using System;
    using System.Net.Sockets;
    using Client_Side_ChatApp.Models;

    public class TcpChatService
    {
        private TcpClient _client;
        private string _myUsername;

        // 1. This replaces your global user setup
        public void ConnectAndSetUser(string username, string serverIp, int port)
        {
            _myUsername = username;

            // Initialize TCP Client here
            // _client = new TcpClient(serverIp, port);

            // Send your connection packet: "Tarık 99"
            string authPacket = $"{_myUsername} 99";
            SendRawData(authPacket);
        }

        // 2. This formats and sends the standard messages
        public void SendMessage(string receiver, string content)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
            string packet = $"{date} {_myUsername} {receiver} {content}";

            SendRawData(packet);
        }

        // 3. The actual low-level socket writer
        private void SendRawData(string data)
        {
            // NetworkStream stream = _client.GetStream();
            // byte[] buffer = Encoding.UTF8.GetBytes(data);
            // stream.Write(buffer, 0, buffer.Length);
        }
    }
}