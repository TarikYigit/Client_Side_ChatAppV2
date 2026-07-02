using ClientSideChatApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientSideChatApp.Core
{
    enum MessageId : byte
    {
        Authenticate = 1,
        RequestUserList = 2,
        ChatMessage = 3,
        FETCH_OFFLINE_MESSAGES = 6
    }

    public class TcpChatService
    {
        private TcpClient _client;
        private string _myUsername;

        public event Action<byte, string> MessageReceived;
        public event Action<Dictionary<byte, string>> UserListUpdated;
        public event Action<byte> LoginSuccessful;
        public event Action LoginRejected;

        public Dictionary<byte, string> AllUsers { get; private set; } = new Dictionary<byte, string>();
        public Dictionary<byte, ObservableCollection<MessageModel>> ChatHistories { get; private set; } = new Dictionary<byte, ObservableCollection<MessageModel>>();

        private void SendRawData(byte[] packet)
        {
            NetworkStream stream = _client.GetStream();

            byte messageId = packet[0];
            int payloadLength = packet.Length - 1;

            byte[] finalPacket = new byte[1 + 4 + payloadLength];
            finalPacket[0] = messageId;

            byte[] lengthBytes = BitConverter.GetBytes(payloadLength);
            Array.Copy(lengthBytes, 0, finalPacket, 1, 4);

            if (payloadLength > 0)
            {
                Array.Copy(packet, 1, finalPacket, 5, payloadLength);
            }

            stream.Write(finalPacket, 0, finalPacket.Length);
            stream.Flush();
        }

        public void ConnectAndSetUser(string username, string serverIp)
        {
            _myUsername = username;
            _client = new TcpClient(serverIp, 5000);

            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            byte[] packet = new byte[1 + usernameBytes.Length];
            packet[0] = 0x01; // packet type
            Array.Copy(usernameBytes, 0, packet, 1, usernameBytes.Length);

            SendRawData(packet);
            Task.Run(() => ListenForPackets());
        }

        public void ConnectExistingUser(string username)
        {
            _myUsername = username;
            _client = new TcpClient("127.0.0.1", 5000);

            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            byte[] packet = new byte[1 + usernameBytes.Length];
            packet[0] = 0x05; // packet type
            Array.Copy(usernameBytes, 0, packet, 1, usernameBytes.Length);

            SendRawData(packet);
            Task.Run(() => ListenForPackets());
        }

        public void RequestClientList(byte myUserId)
        {
            byte[] request_Packet = new byte[] { 0x02, myUserId };
            SendRawData(request_Packet);
        }

        public void SendMessage(byte senderId, byte receiverId, string content)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            byte[] packet = new byte[3 + contentBytes.Length];
            packet[0] = 0x03; // packet type
            packet[1] = senderId;
            packet[2] = receiverId;
            Array.Copy(contentBytes, 0, packet, 3, contentBytes.Length);

            SendRawData(packet);
        }

        public void FetchMissedMessages(byte myUserId)
        {
            byte[] request_Packet = new byte[] { (byte)MessageId.FETCH_OFFLINE_MESSAGES, myUserId };
            SendRawData(request_Packet);
        }

        public void Disconnect()
        {
            byte[] disconnect_Packet = new byte[] { 0x04 };
            SendRawData(disconnect_Packet);
            _client?.Close();
        }

        private void ListenForPackets()
        {
            NetworkStream stream = _client.GetStream();
            BinaryReader networkReader = new BinaryReader(stream, Encoding.UTF8, true);

            try
            {
                while (_client.Connected)
                {
                    byte packetType;
                    int payloadLength;

                    try
                    {
                        packetType = networkReader.ReadByte();
                        payloadLength = networkReader.ReadInt32(); 
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }

                    byte[] payload = networkReader.ReadBytes(payloadLength);

                    switch ((MessageId)packetType)
                    {
                        case MessageId.Authenticate:
                            {
                                byte accept_reject = payload[0];

                                if (accept_reject == 0x01)
                                {
                                    byte assignedUserId = payload[1];
                                    string user_ID_string = assignedUserId.ToString();

                                    try
                                    {
                                        string filePath = @"C:\Users\tarik.dalkiran\Desktop\user_ID_file.txt";
                                        string fileContent = $"{_myUsername} {user_ID_string}";
                                        System.IO.File.AppendAllText(filePath, fileContent + "\n");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[Warning] Could not save credential file: {ex.Message}");
                                    }

                                    LoginSuccessful?.Invoke(assignedUserId);
                                }
                                else // rejected
                                {
                                    stream.Close();
                                    _client.Close();
                                    LoginRejected?.Invoke();
                                    return;
                                }
                            }
                            break;

                        case MessageId.RequestUserList:
                            {
                                using (MemoryStream ms = new MemoryStream(payload))
                                using (BinaryReader payloadReader = new BinaryReader(ms))
                                {
                                    byte userCount = payloadReader.ReadByte();
                                    Dictionary<byte, string> incomingUsers = new Dictionary<byte, string>();

                                    for (int i = 0; i < userCount; i++)
                                    {
                                        byte userId = payloadReader.ReadByte();
                                        byte nameLength = payloadReader.ReadByte();
                                        byte[] nameBuffer = payloadReader.ReadBytes(nameLength);
                                        string username = Encoding.UTF8.GetString(nameBuffer);

                                        incomingUsers[userId] = username;
                                    }

                                    AllUsers = incomingUsers;
                                    UserListUpdated?.Invoke(AllUsers);
                                }
                            }
                            break;

                        case MessageId.ChatMessage:
                            {
                                byte senderId = payload[0];

                                byte[] msgBuffer = new byte[payload.Length - 1];
                                Array.Copy(payload, 1, msgBuffer, 0, msgBuffer.Length);
                                string message = Encoding.UTF8.GetString(msgBuffer);

                                string senderName = AllUsers.ContainsKey(senderId) ? AllUsers[senderId] : $"User_{senderId}";

                                string folderPath = $@"C:\Users\tarik.dalkiran\Desktop\Workspace\ChatLogs_{_myUsername}";
                                System.IO.Directory.CreateDirectory(folderPath);

                                string chatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{senderId}.txt");
                                System.IO.File.AppendAllText(chatFilePath, $"{senderName}|{message}\n");

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (!ChatHistories.ContainsKey(senderId))
                                    {
                                        ChatHistories[senderId] = new ObservableCollection<MessageModel>();
                                    }

                                    ChatHistories[senderId].Add(new MessageModel
                                    {
                                        Sender = senderName,
                                        Content = message
                                    });
                                });

                                MessageReceived?.Invoke(senderId, message);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Network Disconnected]: {ex.Message}");
            }
        }
    }
}