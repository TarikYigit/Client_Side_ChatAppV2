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

        TYPING_STATUS = 12,

        MESSAGE_SENT = 13,

        MESSAGE_SEEN = 14,

        EDIT_MESSAGE = 16,

        EDIT_GROUP_MESSAGE = 17,

        SEND_IMAGE = 18,    

        GROUP_IMAGE = 19

    }

    public class TcpChatService
    {

        private TcpClient _client;

        private string _myUsername;

        private string _myPassword;

        public event Action<byte, int, string> MessageEdited;

        public event Action<byte, byte, int, string> GroupMessageEdited;

        public event Action<byte, int, string, string> MessageReceived;

        public event Action<List<UserModel>> UserListUpdated;
        public List<UserModel> AllUsers { get; private set; } = new List<UserModel>();

        public List<GroupModel> AllGroups { get; private set; } = new List<GroupModel>();

        public event Action<byte> LoginSuccessful;

        public event Action<byte> RegisterSuccessful;

        public event Action<byte, byte, int, string, string, string> GroupMessageReceived;
        public event Action LoginRejected;

        public event Action RegisterRejectedUsername;

        public event Action RegisterRejectedPassword;

        public event Action<List<GroupModel>> GroupListUpdated;

        public event Action<byte> UserIsTypingReceived;

        public event Action<int, bool> MessageStatusChanged;
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

        public void SendMessage(byte senderId, byte receiverId, string content, int messageid)
        {

            ChatMessageRequest request = new ChatMessageRequest(senderId, receiverId, content, messageid);

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

                    // THE FIX 1: Guaranteed chunk reader. This forces TCP to wait for the entire image to download!
                    byte[] payload = new byte[payloadLength];
                    int totalBytesRead = 0;
                    while (totalBytesRead < payloadLength)
                    {
                        int bytesRead = stream.Read(payload, totalBytesRead, payloadLength - totalBytesRead);
                        if (bytesRead == 0) throw new EndOfStreamException("Disconnected while reading payload.");
                        totalBytesRead += bytesRead;
                    }

                    // THE FIX 2: Inner try-catch. If one bad packet arrives, it won't crash the entire background listener.
                    try
                    {
                        switch ((MessageId)packetType)
                        {
                            case MessageId.LOG_IN:
                                {
                                    LoginResponse response = new LoginResponse(payload);
                                    bool flowControl = false;

                                    // THE FIX 3: Push all UI event invocations to the Main Window thread!
                                    Application.Current.Dispatcher.Invoke(() => {
                                        flowControl = SetupUser(stream, response);
                                    });

                                    if (!flowControl) return;
                                }
                                break;

                            case MessageId.REQUEST_USER_LIST:
                                {
                                    UserListResponse response = new UserListResponse(payload);

                                    Application.Current.Dispatcher.Invoke(() => {
                                        AllUsers = response.Users;
                                        UserListUpdated?.Invoke(AllUsers);
                                    });
                                }
                                break;

                            case MessageId.CHAT_MESSAGE:
                                {
                                    ChatMessageResponse response = new ChatMessageResponse(payload);
                                    byte senderId = response.SenderId;
                                    int messageId = response.Messageid;
                                    string message = EncryptionManager.DecryptMessage(response.Message);
                                    string timeString = response.TimeStamp.ToLocalTime().ToString("yyyy:MM:dd:HH:mm:ss");

                                    Application.Current.Dispatcher.Invoke(() => {
                                        MessageReceived?.Invoke(senderId, messageId, message, timeString);
                                    });
                                }
                                break;

                            case MessageId.REGISTER:
                                {
                                    UserRegisterResponse response = new UserRegisterResponse(payload);
                                    bool flowControl = false;

                                    Application.Current.Dispatcher.Invoke(() => {
                                        flowControl = ReSetupUser(stream, response);
                                    });

                                    if (!flowControl) return;
                                }
                                break;

                            case MessageId.GROUP_CHAT_MESSAGE:
                                {
                                    GroupChatMessageResponse response = new GroupChatMessageResponse(payload);
                                    byte senderId = response.SenderId;
                                    byte groupId = response.GroupId;
                                    int messageId = response.messageId;
                                    string message = EncryptionManager.DecryptMessage(response.Message);
                                    string timeString = response.TimeStamp.ToLocalTime().ToString("yyyy:MM:dd:HH:mm:ss");

                                    Application.Current.Dispatcher.Invoke(() => {
                                        UserModel sender = AllUsers.Find(u => u.UserId == senderId);
                                        string senderName = sender != null ? sender.Username : $"User_{senderId}";
                                        GroupMessageReceived?.Invoke(groupId, senderId, messageId, senderName, message, timeString);
                                    });
                                }
                                break;

                            case MessageId.REQUEST_GROUP_LIST:
                                {
                                    GroupListResponse response = new GroupListResponse(payload);

                                    Application.Current.Dispatcher.Invoke(() => {
                                        AllGroups = response.Groups;
                                        GroupListUpdated?.Invoke(response.Groups);
                                    });
                                }
                                break;

                            case MessageId.TYPING_STATUS:
                                {
                                    byte typerId = payload[0];
                                    Application.Current.Dispatcher.Invoke(() => {
                                        UserIsTypingReceived?.Invoke(typerId);
                                    });
                                }
                                break;

                            case MessageId.MESSAGE_SENT:
                                {
                                    int deliveredMsgId = BitConverter.ToInt32(payload, 0);
                                    Application.Current.Dispatcher.Invoke(() => {
                                        MessageStatusChanged?.Invoke(deliveredMsgId, false);
                                    });
                                }
                                break;

                            case MessageId.MESSAGE_SEEN:
                                {
                                    int seenMsgId = BitConverter.ToInt32(payload, 0);
                                    Application.Current.Dispatcher.Invoke(() => {
                                        MessageStatusChanged?.Invoke(seenMsgId, true);
                                    });
                                }
                                break;

                            case MessageId.EDIT_MESSAGE:
                                {
                                    ChatMessageResponse response = new ChatMessageResponse(payload);
                                    int messageId = response.Messageid;
                                    byte senderId = response.SenderId;
                                    string newContent = EncryptionManager.DecryptMessage(response.Message);

                                    Application.Current.Dispatcher.Invoke(() => {
                                        MessageEdited?.Invoke(senderId, messageId, newContent);
                                    });
                                }
                                break;

                            case MessageId.EDIT_GROUP_MESSAGE:
                                {
                                    GroupChatMessageResponse response = new GroupChatMessageResponse(payload);
                                    int messageId = response.messageId;
                                    byte senderId = response.SenderId;
                                    byte groupId = response.GroupId;
                                    string newContent = EncryptionManager.DecryptMessage(response.Message);

                                    Application.Current.Dispatcher.Invoke(() => {
                                        GroupMessageEdited?.Invoke(groupId, senderId, messageId, newContent);
                                    });
                                }
                                break;

                            case MessageId.SEND_IMAGE:
                                {
                                    ImageMessageResponse response = new ImageMessageResponse(payload);
                                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                                    string imageFolder = Path.Combine(appData, "ClientSideChatApp", "Images");
                                    Directory.CreateDirectory(imageFolder);

                                    string savePath = Path.Combine(imageFolder, Guid.NewGuid().ToString() + ".png");

                                    // We do the heavy file saving in the background thread...
                                    File.WriteAllBytes(savePath, response.ImageBytes);

                                    string formattedMessage = $"[IMG:{savePath}]";
                                    string timeString = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");

                                    // ...and only push the UI update to the Main Thread!
                                    Application.Current.Dispatcher.Invoke(() => {
                                        MessageReceived?.Invoke(response.SenderId, response.Messageid, formattedMessage, timeString);
                                    });
                                }
                                break;

                            case MessageId.GROUP_IMAGE:
                                {
                                    GroupImageMessageResponse response = new GroupImageMessageResponse(payload);
                                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                                    string imageFolder = Path.Combine(appData, "ClientSideChatApp", "Images");
                                    Directory.CreateDirectory(imageFolder);

                                    string savePath = Path.Combine(imageFolder, Guid.NewGuid().ToString() + ".png");

                                    File.WriteAllBytes(savePath, response.ImageBytes);

                                    Application.Current.Dispatcher.Invoke(() => {
                                        UserModel sender = AllUsers.Find(u => u.UserId == response.SenderId);
                                        string senderName = sender != null ? sender.Username : $"User_{response.SenderId}";
                                        string timeString = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");

                                        GroupMessageReceived?.Invoke(response.GroupId, response.SenderId, response.Messageid, senderName, $"[IMG:{savePath}]", timeString);
                                    });
                                }
                                break;
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Inner Parsing Crash] ID: {packetType}, Error: {innerEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Network Disconnected]: {ex.Message}");
            }
        }

        public void SendReadReceipt(byte originalSenderId, int messageId)
        {
            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(originalSenderId); 

                writer.Write(messageId);       

                SendPacket((byte)MessageId.MESSAGE_SEEN, ms.ToArray());

            }
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

        public void SendGroupMessage(byte senderId, byte groupId,  int messageid, string content)
        {
            GroupChatMessageRequest request = new GroupChatMessageRequest(senderId, groupId, messageid, content);

            SendPacket(request.GetId(), request.ToBytes());
        }

        public void RequestGroupList(byte myUserId)
        {

            SimpleByteRequest request = new SimpleByteRequest((byte)MessageId.REQUEST_GROUP_LIST, myUserId);

            SendPacket(request.GetId(), request.ToBytes());

        }

        public void LeaveGroup(byte userId, byte groupId)
        {

            LeaveGroupRequest request = new LeaveGroupRequest(userId, groupId);

            byte[] payload = request.ToBytes();

            byte messageId = request.GetId();

            SendPacket(messageId, payload);

        }

        public void AddUserToGroup(byte groupId, byte newUserId)
        {

            AddUserToGroupRequest request = new AddUserToGroupRequest(groupId, newUserId);

            byte[] payload = request.ToBytes();

            byte messageId = request.GetId();

            SendPacket(messageId, payload);

        }

        public void SendTypingStatus(byte myId, byte targetId)
        {

            SendPacket((byte)MessageId.TYPING_STATUS, new byte[] { targetId });

        }

        public void SendEditMessage(byte senderId, byte receiverId, int messageId, string newContent)
        {

            ChatMessageRequest request = new ChatMessageRequest(senderId, receiverId, newContent, messageId);

            SendPacket((byte)MessageId.EDIT_MESSAGE, request.ToBytes());

        }

        public void SendEditGroupMessage(byte senderId, byte groupId, int messageId, string newContent)
        {

            GroupChatMessageRequest request = new GroupChatMessageRequest(senderId, groupId, messageId, newContent);

            SendPacket((byte)MessageId.EDIT_GROUP_MESSAGE, request.ToBytes());

        }

        public void SendImageMessage(byte senderId, byte receiverId, int messageId, byte[] imageBytes)
        {
            try
            {
                SendImageRequest request = new SendImageRequest(senderId, receiverId, messageId, imageBytes);
                SendPacket(request.GetId(), request.ToBytes());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Network Packet Crash: {ex.Message}", "TCP Error");
            }
        }

        public void SendGroupImageMessage(byte senderId, byte groupId, int messageId, byte[] imageBytes)
        {
            try
            {
                GroupImageRequest request = new GroupImageRequest(senderId, groupId, messageId, imageBytes);
                SendPacket(request.GetId(), request.ToBytes());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Network Group Packet Crash: {ex.Message}", "TCP Error");
            }
        }
    }
}