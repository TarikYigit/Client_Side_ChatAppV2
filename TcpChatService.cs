using Client_Side_ChatApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client_Side_ChatApp.Core
{
    enum MessageId : byte
    {
        Authenticate = 1,
        RequestUserList = 2,
        ChatMessage = 3,
        FETCH_OFFLINE_MESSAGES = 6 // Added the new command
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

        //user setup
        public void ConnectAndSetUser(string username, string serverIp)
        {
            _myUsername = username;

            _client = new TcpClient(serverIp, 5000);
            NetworkStream stream = _client.GetStream();

            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);

            List<byte> packetList = new List<byte>();
            packetList.Add(0x01);       //packet type
            packetList.AddRange(BitConverter.GetBytes(usernameBytes.Length));
            packetList.AddRange(usernameBytes);

            byte[] authenticate_Packet = packetList.ToArray();

            // Send connection packet
            SendRawData(authenticate_Packet);
            Task.Run(() => ListenForPackets());
        }

        //sent automatically once connected for the first time or logging in again
        public void RequestClientList(byte myUserId)
        {
            byte[] request_Packet = new byte[] { 0x02, myUserId };
            SendRawData(request_Packet);
        }

        //this formats and sends the standard messages
        public void SendMessage(byte senderId, byte receiverId, string content)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            List<byte> packetList = new List<byte>();
            packetList.Add(0x03);       // packet type
            packetList.Add(senderId);
            packetList.Add(receiverId);

            int length = contentBytes.Length;
            if (length > 255) length = 255; // Cap at 1 byte

            packetList.Add((byte)length);

            for (int i = 0; i < length; i++)
            {
                packetList.Add(contentBytes[i]);
            }

            byte[] message_Packet = packetList.ToArray();
            SendRawData(message_Packet);
        }

        public void FetchMissedMessages(byte myUserId)
        {
            byte[] request_Packet = new byte[] { (byte)MessageId.FETCH_OFFLINE_MESSAGES, myUserId };
            SendRawData(request_Packet);
        }

        //is sent automatically once the connection to the server is terminated
        public void Disconnect()
        {
            // Server only needs the 0x04 byte
            byte[] disconnect_Packet = new byte[] { 0x04 };
            SendRawData(disconnect_Packet);

            _client?.Close();
        }

        //is sent automatically if a certain file that holds the user name and user ID assigned by the server exists on the clients device
        public void ConnectExistingUser(string username)
        {
            _myUsername = username;

            _client = new TcpClient("127.0.0.1", 5000);

            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            List<byte> packetList = new List<byte>();

            packetList.Add(0x05);       // packet type
            packetList.AddRange(BitConverter.GetBytes(usernameBytes.Length));
            packetList.AddRange(usernameBytes);

            byte[] authenticate_Packet = packetList.ToArray();

            SendRawData(authenticate_Packet);
            Task.Run(() => ListenForPackets());
        }

        //sends the packet
        private void SendRawData(byte[] data)
        {
            NetworkStream stream = _client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        //initiated when case 1 or case 5 happens automatically to set up te client side of the app
        private void ListenForPackets()
        {
            NetworkStream stream = _client.GetStream();

            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);

            try
            {
                while (_client.Connected)
                {
                    byte packetType;
                    try
                    {
                        packetType = reader.ReadByte();
                    }
                    catch (EndOfStreamException)
                    {
                        break; // Connection closed gracefully by the server
                    }

                    switch ((MessageId)packetType)
                    {
                        case MessageId.Authenticate: // Authentication Response from Server
                            byte accept_reject = reader.ReadByte();

                            if (accept_reject == 0x01) // accepted
                            {
                                byte assignedUserId = reader.ReadByte();
                                string user_ID_string = assignedUserId.ToString();

                                try
                                {
                                    string filePath = @"C:\Users\tarik.dalkiran\Desktop\user_ID_file.txt";
                                    string fileContent = $"{_myUsername} {user_ID_string}";
                                    System.IO.File.AppendAllText(filePath, fileContent + "\n");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Warning] Could not save credential file: {ex.Message}");
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
                            break;

                        case MessageId.RequestUserList: // Incoming Full User List
                            byte userCount = reader.ReadByte();
                            Dictionary<byte, string> incomingUsers = new Dictionary<byte, string>();

                            for (int i = 0; i < userCount; i++)
                            {
                                byte userId = reader.ReadByte();
                                byte nameLength = reader.ReadByte();
                                byte[] nameBuffer = reader.ReadBytes(nameLength);
                                string username = Encoding.UTF8.GetString(nameBuffer);

                                incomingUsers[userId] = username;
                            }

                            AllUsers = incomingUsers;
                            UserListUpdated?.Invoke(AllUsers);
                            break;

                        case MessageId.ChatMessage: // Incoming Chat Message
                            byte senderId = reader.ReadByte();
                            byte msgLength = reader.ReadByte();

                            byte[] msgBuffer = reader.ReadBytes(msgLength);

                            string message = Encoding.UTF8.GetString(msgBuffer);
                            string senderName = AllUsers.ContainsKey(senderId) ? AllUsers[senderId] : $"User_{senderId}";

                            string folderPath = $@"C:\Users\tarik.dalkiran\Desktop\Workspace\ChatLogs_{_myUsername}";
                            System.IO.Directory.CreateDirectory(folderPath);

                            string chatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{senderId}.txt");
                            System.IO.File.AppendAllText(chatFilePath, $"{senderName}|{message}\n");

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (!ChatHistories.ContainsKey(senderId))
                                {
                                    ChatHistories[senderId] = new System.Collections.ObjectModel.ObservableCollection<MessageModel>();
                                }

                                ChatHistories[senderId].Add(new MessageModel
                                {
                                    Sender = senderName,
                                    Content = message
                                });
                            });

                            MessageReceived?.Invoke(senderId, message);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Ensures a sudden disconnect doesn't crash the UI
                Console.WriteLine($"[Network Disconnected]: {ex.Message}");
            }
        }
    }
}