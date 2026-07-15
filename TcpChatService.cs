using Client_Side_ChatApp.Messages;
using ClientSideChatApp.Messages;
using ClientSideChatApp.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;


namespace ClientSideChatApp.Core
{

    public enum MessageId : byte
    {

        REGISTER = 1,    

        REQUEST_USER_LIST = 2,     

        CHAT_MESSAGE = 3,        

        DISCONNECT = 4,    

        LOG_IN = 5,                     

        FETCH_OFFLINE_MESSAGES = 6,      

        CREATE_GROUP = 7,

        GROUP_CHAT_MESSAGE = 8,

        REQUEST_GROUP_LIST = 9,

        LEAVE_GROUP = 10,

        ADD_USER_TO_GROUP = 11,

    }

    public class TcpChatService
    {

        private TcpClient _client;

        private string _myUsername;

        private string _myPassword;

        public event Action<byte, string, string> MessageReceived;

        public event Action<List<UserModel>> UserListUpdated;
        public List<UserModel> AllUsers { get; private set; } = new List<UserModel>();

        public event Action<byte> LoginSuccessful;

        public event Action<byte> RegisterSuccessful;

        public event Action<byte, string, string, string> GroupMessageReceived; 

        public event Action LoginRejected;

        public event Action RegisterRejectedUsername;

        public event Action RegisterRejectedPassword;

        public event Action<List<GroupModel>> GroupListUpdated;

        public Dictionary<byte, ObservableCollection<MessageModel>>  ChatHistories { get; private set; } = new Dictionary<byte, ObservableCollection<MessageModel>>();



        private void SendPacket(byte messageId, byte[] payload)
        {

            int payloadLength = payload?.Length ?? 0;

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(messageId);

                writer.Write(payloadLength); 

                if (payloadLength > 0)
                {

                    writer.Write(payload);

                }

                byte[] finalPacket = ms.ToArray();

                NetworkStream stream = _client.GetStream();

                stream.Write(finalPacket, 0, finalPacket.Length);

                stream.Flush();

            }
        }



        public void ConnectAndRegister(string username, string password)
        {

            _myUsername = username;

            _myPassword = password;

            _client = new TcpClient("127.0.0.1", 5000);

            UserRegisterRequest request = new UserRegisterRequest(username, password);

            SendPacket(request.GetId(), request.ToBytes());

            Task.Run(() => ListenForPackets());

        }



        public void ConnectAndLogin(string username, string password)
        {

            _myUsername = username;

            _myPassword = password;

            _client = new TcpClient("127.0.0.1", 5000);

            UserLoginRequest request = new UserLoginRequest(username, password);

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

                                bool flowControl = SetupUser(stream, response);

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

                                string timeString;

                                SaveMessagesFromOthersOnClientsPC(response, out senderId, out message, out senderName, out timeString);

                                UpdateUIForClient(senderId, message, senderName, timeString);

                                MessageReceived?.Invoke(senderId, message, timeString);

                            }
                            break;

                        case MessageId.REGISTER:
                            {   
                                UserRegisterResponse response = new UserRegisterResponse(payload);

                                bool flowControl = ReSetupUser(stream, response);

                                if (!flowControl)
                                {

                                    return;

                                }
                            }
                            break;

                        case MessageId.GROUP_CHAT_MESSAGE:
                            {
                                GroupChatMessageResponse response = new GroupChatMessageResponse(payload);

                                byte senderId = response.SenderId;

                                byte groupId = response.GroupId;

                                string message = EncryptionManager.DecryptMessage(response.Message);

                                string timeString = response.TimeStamp.ToLocalTime().ToString("yyyy:MM:dd:HH:mm:ss");

                                UserModel sender = AllUsers.Find(u => u.UserId == senderId);

                                string senderName = sender != null ? sender.Username : $"User_{senderId}";

                                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                                string folderPath = System.IO.Path.Combine(appData, "ClientSideChatApp", $"ChatLogs_{_myUsername}");

                                System.IO.Directory.CreateDirectory(folderPath);

                                string chatFilePath = System.IO.Path.Combine(folderPath, $"GroupChat_{groupId}.txt");

                                System.IO.File.AppendAllText(chatFilePath, $"{senderName}|{timeString}|{message}\n");

                                GroupMessageReceived?.Invoke(groupId, senderName, message, timeString);
                            }
                            break;

                        case MessageId.REQUEST_GROUP_LIST:
                            {

                                GroupListResponse response = new GroupListResponse(payload);

                                GroupListUpdated?.Invoke(response.Groups);

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



        private void UpdateUIForClient(byte senderId, string message, string senderName, string timeString)
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

                    Timestamp = timeString,

                    Content = message

                });
            });
        }



        private bool SetupUser(NetworkStream stream, LoginResponse response)
        {

            if (response.IsAccepted)
            {

                byte assignedUserId = response.AssignedUserId;

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

        private bool ReSetupUser(NetworkStream stream, UserRegisterResponse response)
        {

            if (response.IsAccepted)
            {

                byte assignedUserId = response.AssignedUserId;

                LoginSuccessful?.Invoke(assignedUserId);

                return true;

            }
            else
            {

                stream.Close();

                _client.Close();

                if (response.IsPasswordWeak)
                {

                    RegisterRejectedPassword?.Invoke();

                }
                else
                {

                    RegisterRejectedUsername?.Invoke();

                }

                return false;

            }
        }

        public void CreateGroup(string groupName, List<byte> userIdsToInvite)
        {

            CreateGroupRequest request = new CreateGroupRequest(groupName, userIdsToInvite);

            SendPacket(request.GetId(), request.ToBytes());

        }

        public void SendGroupMessage(byte senderId, byte groupId, string content)
        {
            GroupChatMessageRequest request = new GroupChatMessageRequest(senderId, groupId, content);

            SendPacket(request.GetId(), request.ToBytes());
        }

        public void RequestGroupList(byte myUserId)
        {

            SimpleByteRequest request = new SimpleByteRequest((byte)MessageId.REQUEST_GROUP_LIST, myUserId);

            SendPacket(request.GetId(), request.ToBytes());

        }

        private void SaveMessagesFromOthersOnClientsPC(ChatMessageResponse response, out byte senderId, out string message, out string senderName, out string timeString)
        {
            senderId = response.SenderId;

            message = EncryptionManager.DecryptMessage(response.Message);

            timeString = response.TimeStamp.ToLocalTime().ToString("yyyy:MM:dd:HH:mm:ss");

            UserModel sender = AllUsers.Find(u => u.UserId == response.SenderId);

            senderName = sender != null ? sender.Username : $"User_{response.SenderId}";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string folderPath = System.IO.Path.Combine(appData, "ClientSideChatApp", $"ChatLogs_{_myUsername}");

            System.IO.Directory.CreateDirectory(folderPath);

            string chatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{response.SenderId}.txt");

            System.IO.File.AppendAllText(chatFilePath, $"{senderName}|{timeString}|{message}\n");

        }
        public void LeaveGroup(byte myUserId, byte groupId)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())

            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(ms))
            {

                writer.Write(myUserId);

                writer.Write(groupId);

                SendPacket((byte)MessageId.LEAVE_GROUP, ms.ToArray());

            }
        }

        public void AddUserToGroup(byte groupId, byte userToAddId)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())

            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(ms))
            {

                writer.Write(groupId);

                writer.Write(userToAddId);

                SendPacket((byte)MessageId.ADD_USER_TO_GROUP, ms.ToArray());

            }
        }
    }
}