using System;
using System.Collections.ObjectModel;
using System.Linq; // Added for Select()
using System.Windows;
using ClientSideChatApp.Core;
using ClientSideChatApp.Models;

namespace ClientSideChatApp.ViewModels
{
    public class GroupChatViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        private TcpChatService _chatService;

        private string _currentChatFilePath;

        private List<UserModel> _availableUsers;

        private int? _editingMessageId = null;
        public RelayCommand BeginEditCommand { get; set; }
        public GroupModel TargetGroup { get; set; }
        public string HeaderText => $"Group Chat: {TargetGroup.GroupName}";

        public ObservableCollection<MessageModel> Messages { get; set; }

        private string _inputText;
        public string InputText
        {

            get { return _inputText; }

            set { _inputText = value; OnPropertyChanged(); }

        }

        public RelayCommand SendCommand { get; set; }
        public RelayCommand BackCommand { get; set; }

        public RelayCommand DeleteMessageCommand { get; set; }
        public RelayCommand AddUserToGroupCommand { get; set; }
        public RelayCommand LeaveGroupCommand { get; set; }

        public GroupChatViewModel(MainViewModel mainViewModel, TcpChatService chatService, GroupModel targetGroup)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            TargetGroup = targetGroup;

            InitializeChat();

        }

        private void InitializeChat()
        {

            Messages = new ObservableCollection<MessageModel>();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string folderPath = System.IO.Path.Combine(appData, "ClientSideChatApp", $"ChatLogs_{_mainViewModel.MyUsername}");

            System.IO.Directory.CreateDirectory(folderPath);

            _currentChatFilePath = System.IO.Path.Combine(folderPath, $"GroupChat_{TargetGroup.GroupId}.txt");

            LoadChatHistory();

            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);

            BackCommand = new RelayCommand(ExecuteBack);

            AddUserToGroupCommand = new RelayCommand(ExecuteAddUser);

            LeaveGroupCommand = new RelayCommand(ExecuteLeaveGroup);

            _chatService.GroupMessageReceived += OnGroupMessageReceived;

            _chatService.MessageStatusChanged += OnMessageStatusChanged;

            _chatService.FetchMissedMessages(_mainViewModel.MyUserId);

            BeginEditCommand = new RelayCommand(ExecuteBeginEdit);

            _chatService.GroupMessageEdited += OnGroupMessageEdited;

            DeleteMessageCommand = new RelayCommand(ExecuteDeleteMessage);
        }

        private void ExecuteBeginEdit(object parameter)
        {

            if (parameter is MessageModel msg && msg.IsMyMessage)
            {

                if (msg.Content == "🚫 This message was deleted") return;

                InputText = msg.Content;

                _editingMessageId = msg.MessageId;

            }
        }

        private void OnGroupMessageEdited(byte groupId, byte senderId, int messageId, string newContent)
        {

            if (TargetGroup != null && groupId == TargetGroup.GroupId)
            {

                Application.Current.Dispatcher.Invoke(() =>
                {

                    var msg = Messages.FirstOrDefault(m => m.MessageId == messageId);

                    if (msg != null)
                    {

                        msg.Content = newContent;

                        msg.IsEdited = true;

                        SyncChatFile();
                    }
                });
            }
        }

        private void SyncChatFile()
        {

            var lines = Messages.Select(m => $"{m.Sender}|{m.Timestamp}|{m.MessageId}|{m.Content}|{m.IsSeen}");

            System.IO.File.WriteAllLines(_currentChatFilePath, lines);

        }

        private void LoadChatHistory()
        {

            if (System.IO.File.Exists(_currentChatFilePath))
            {

                string[] savedMessages = System.IO.File.ReadAllLines(_currentChatFilePath);

                foreach (string line in savedMessages)
                {

                    string[] parts = line.Split(new char[] { '|' }, 5);

                    if (parts.Length == 5)
                    {

                        Messages.Add(new MessageModel
                        {

                            Sender = parts[0],

                            Timestamp = parts[1],

                            MessageId = int.Parse(parts[2]),

                            Content = parts[3],

                            IsSent = true,

                            IsSeen = bool.Parse(parts[4]),

                            IsMyMessage = parts[0] == _mainViewModel.MyUsername

                        });
                    }
                }
            }
        }

        private void OnGroupMessageReceived(byte groupId, byte senderId, int messageId, string senderName, string messageContent, string timeString)
        {

            if (groupId == TargetGroup.GroupId)
            {

                Application.Current.Dispatcher.Invoke(() =>
                {

                    Messages.Add(new MessageModel
                    {

                        MessageId = messageId,

                        Sender = senderName,

                        Timestamp = timeString,

                        Content = messageContent,

                        IsSent = true,

                        IsSeen = true,

                        IsMyMessage = false

                    });

                    SyncChatFile();

                    if (senderId != _mainViewModel.MyUserId)
                    {

                        _chatService.SendReadReceipt(senderId, messageId);

                    }
                });
            }
        }

        private void OnMessageStatusChanged(int messageId, bool isRead)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                var msg = Messages.FirstOrDefault(m => m.MessageId == messageId);

                if (msg != null)
                {

                    if (isRead)

                        msg.IsSeen = true;
                    else

                        msg.IsSent = true;

                    SyncChatFile();

                }
            });
        }

        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(InputText);

        private void ExecuteSend(object parameter)
        {

            string cipherText = EncryptionManager.EncryptMessage(InputText);

            if (_editingMessageId.HasValue)
            {
                _chatService.SendEditGroupMessage(_mainViewModel.MyUserId, (byte)TargetGroup.GroupId, _editingMessageId.Value, cipherText);

                var msgToEdit = Messages.FirstOrDefault(m => m.MessageId == _editingMessageId.Value);

                if (msgToEdit != null)
                {

                    msgToEdit.Content = InputText;

                    msgToEdit.IsEdited = true;

                    SyncChatFile();

                }

                _editingMessageId = null;

                InputText = string.Empty;

                return;
            }

            string currentTime = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");

            int generatedMessageId = new Random().Next(1, int.MaxValue);

            _chatService.SendGroupMessage(_mainViewModel.MyUserId, (byte)TargetGroup.GroupId, generatedMessageId, cipherText);

            Messages.Add(new MessageModel
            {

                MessageId = generatedMessageId,

                Sender = _mainViewModel.MyUsername,

                Timestamp = currentTime,

                Content = InputText,

                IsSent = false,

                IsSeen = false,

                IsMyMessage = true

            });

            SyncChatFile();
            InputText = string.Empty;
        }

        private void ExecuteDeleteMessage(object parameter)
        {
            if (parameter is MessageModel msg && msg.IsMyMessage)
            {

                string deletedText = "🚫 This message was deleted";

                string cipherText = EncryptionManager.EncryptMessage(deletedText);

                _chatService.SendEditGroupMessage(_mainViewModel.MyUserId, (byte)TargetGroup.GroupId, msg.MessageId, cipherText);

                msg.Content = deletedText;

                msg.IsEdited = false;

                SyncChatFile();
            }
        }

        private void ExecuteAddUser(object parameter)
        {

            var availableUsers = new ObservableCollection<UserModel>(_chatService.AllUsers.Where(u => u.UserId != _mainViewModel.MyUserId));

            _mainViewModel.CurrentView = new AddUserToGroupViewModel(_mainViewModel, _chatService, TargetGroup, availableUsers);

        }

        private void ExecuteLeaveGroup(object parameter)
        {

            _chatService.LeaveGroup(_mainViewModel.MyUserId, (byte)TargetGroup.GroupId);

            ExecuteBack(null);

        }

        private void ExecuteBack(object parameter)
        {
            _chatService.GroupMessageReceived -= OnGroupMessageReceived;

            _chatService.MessageStatusChanged -= OnMessageStatusChanged;

            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

        }
    }
}