using Client_Side_ChatApp.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
namespace Client_Side_ChatApp.Core
{

    public class TcpChatService
    {
        private TcpClient _client;
        private string _myUsername;

        //user setup
        public void ConnectAndSetUser(string username, string serverIp, int port)
        {
            _myUsername = username;

            // Initialize TCP Client
            _client = new TcpClient(serverIp, 5000);
            NetworkStream stream = _client.GetStream();

            string authenticate_Packet = $"{_myUsername} 99";            // send connection packet
            SendRawData(authenticate_Packet);
        }

        //this formats and sends the standard messages
        public void SendMessage(string receiver, string content)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
            string packet = $"{date} {_myUsername} {receiver} {content}";
            SendRawData(packet);
        }
        public void ConnectToRoom(string Who, string To_Who)
        {
            string packet = Who  + " to " + To_Who;
            SendRawData(packet);
        }
        public void Disconnect(string receiver, string content)
        {
            string Disconnect_Packet = $"{_myUsername} 11";            // send disconnect packet
            SendRawData(Disconnect_Packet);
        }

        private void SendRawData(string data)
        {
            NetworkStream stream = _client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}