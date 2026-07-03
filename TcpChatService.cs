using ClientSideChatApp.Messages;
using ClientSideChatApp.Messages;
using ClientSideChatApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientSideChatApp.Core
{
    public enum MessageId : byte
    {
        LOG_IN = 1,                 // 0x01 
        REQUEST_USER_LIST = 2,      // 0x02
        CHAT_MESSAGE = 3,           // 0x03
        DISCONNECT = 4,             // 0x04
        EXISTING_USER_LOG_IN = 5,   // 0x05
        FETCH_OFFLINE_MESSAGES = 6  // 0x06
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

        private void SendPacket(byte messageId, byte[] payload)
        {
            int payloadLength = payload?.Length ?? 0;
            byte[] finalPacket = new byte[1 + 4 + payloadLength];

            finalPacket[0] = messageId;

            byte[] lengthBytes = BitConverter.GetBytes(payloadLength);
            Array.Copy(lengthBytes, 0, finalPacket, 1, 4);

            if (payloadLength > 0)
            {
                Array.Copy(payload, 0, finalPacket, 5, payloadLength);
            }

            NetworkStream stream = _client.GetStream();
            stream.Write(finalPacket, 0, finalPacket.Length);
            stream.Flush();
        }

        public void ConnectAndSetUser(string username, string serverIp)
        {
            _myUsername = username;
            _client = new TcpClient(serverIp, 5000);

            LoginRequest request = new LoginRequest(username);
            SendPacket(request.GetId(), request.ToBytes());

            Task.Run(() => ListenForPackets());
        }

        public void ConnectExistingUser(string username)
        {
            _myUsername = username;
            _client = new TcpClient("127.0.0.1", 5000);

            ExistingUserLoginRequest request = new ExistingUserLoginRequest(username);
            SendPacket(request.GetId(), request.ToBytes());

            Task.Run(() => ListenForPackets());
        }

        public void RequestClientList(byte myUserId)
        {
            SimpleByteRequest request = new SimpleByteRequest((byte)MessageId.REQUEST_USER_LIST, myUserId);
            SendPacket(request.GetId(), request.ToBytes());
        }

        public void SendMessage(byte senderId, byte receiverId, string content)
        {
            ChatMessageRequest request = new ChatMessageRequest(senderId, receiverId, content);
            SendPacket(request.GetId(), request.ToBytes());
        }

        public void FetchMissedMessages(byte myUserId)
        {
            SimpleByteRequest request = new SimpleByteRequest((byte)MessageId.FETCH_OFFLINE_MESSAGES, myUserId);
            SendPacket(request.GetId(), request.ToBytes());
        }

        public void Disconnect()
        {
            DisconnectRequest request = new DisconnectRequest();
            SendPacket(request.GetId(), request.ToBytes());
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
                        case MessageId.LOG_IN:
                            {
                                LoginResponse response = new LoginResponse(payload);
                                bool flowControl = CheckIfAcceptedAndSaveUserLoginInfoForRepeatedUse(stream, response);
                                if (!flowControl)
                                {
                                    return;
                                }
                            }
                            break;

                        case MessageId.REQUEST_USER_LIST:
                            {
                                UserListResponse response = new UserListResponse(payload);

                                AllUsers = response.Users;
                                UserListUpdated?.Invoke(AllUsers);
                            }
                            break;

                        case MessageId.CHAT_MESSAGE:
                            {
                                ChatMessageResponse response = new ChatMessageResponse(payload);
                                byte senderId;
                                string message, senderName;

                                SaveMessagesFromOthersOnClientsPC(response, out senderId, out message, out senderName);

                                UpdateUIForClient(senderId, message, senderName);

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

        private void UpdateUIForClient(byte senderId, string message, string senderName)
        {
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
        }

        private bool CheckIfAcceptedAndSaveUserLoginInfoForRepeatedUse(NetworkStream stream, LoginResponse response)
        {
            if (response.IsAccepted)
            {
                byte assignedUserId = response.AssignedUserId;
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
            else
            {
                stream.Close();
                _client.Close();
                LoginRejected?.Invoke();
                return false;
            }

            return true;
        }

        private void SaveMessagesFromOthersOnClientsPC(ChatMessageResponse response, out byte senderId, out string message, out string senderName)
        {
            senderId = response.SenderId;
            message = response.Message;
            senderName = AllUsers.ContainsKey(senderId) ? AllUsers[senderId] : $"User_{senderId}";
            string folderPath = $@"C:\Users\tarik.dalkiran\Desktop\Workspace\ChatLogs_{_myUsername}";
            System.IO.Directory.CreateDirectory(folderPath);

            string chatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{senderId}.txt");
            System.IO.File.AppendAllText(chatFilePath, $"{senderName}|{message}\n");
        }
    }
}