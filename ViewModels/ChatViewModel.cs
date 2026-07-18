using System.Collections.ObjectModel;
using System.Windows;
using ClientSideChatApp.Core;
using ClientSideChatApp.Models;

namespace ClientSideChatApp.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        private TcpChatService _chatService;

        private string _currentChatFilePath;

        public UserModel TargetUser { get; set; }
        public GroupModel TargetGroup { get; set; }

        public string HeaderText
        {
            get
            {

                if (TargetGroup != null) return $"Group Chat: {TargetGroup.GroupName}";

                return TargetUser != null ? $"Chat with {TargetUser.Username}" : "Chat";

            }
        }

        public ObservableCollection<MessageModel> Messages { get; set; }

        private string _inputText;
        public string InputText
        {

            get { return _inputText; }

            set { 
                _inputText = value; 
                OnPropertyChanged();
                if (TargetUser != null && !string.IsNullOrEmpty(_inputText))
                {

                    _chatService.SendTypingStatus(_mainViewModel.MyUserId, TargetUser.UserId);

                }
            }

        }

        private bool _isTyping;
        public bool IsTyping
        {
            get { return _isTyping; }

            set { _isTyping = value; OnPropertyChanged(); }
        }

        public RelayCommand SendCommand { get; set; }
        public RelayCommand BackCommand { get; set; }


        public ChatViewModel(MainViewModel mainViewModel, TcpChatService chatService, UserModel targetUser)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            TargetUser = targetUser;

            InitializeChat();
        }


        public ChatViewModel(MainViewModel mainViewModel, TcpChatService chatService, GroupModel targetGroup)
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

            if (TargetGroup != null)

                _currentChatFilePath = System.IO.Path.Combine(folderPath, $"GroupChat_{TargetGroup.GroupId}.txt");

            else if (TargetUser != null)

                _currentChatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{TargetUser.UserId}.txt");

            LoadChatHistory();

            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);

            BackCommand = new RelayCommand(ExecuteBack);

            _chatService.MessageReceived += OnMessageReceived;

            _chatService.MessageStatusChanged += OnMessageStatusChanged; 

            _chatService.FetchMissedMessages(_mainViewModel.MyUserId);

            _chatService.UserIsTypingReceived += (typerId) =>
            {

                if (TargetUser != null && typerId == TargetUser.UserId)
                {

                    Application.Current.Dispatcher.Invoke(async () =>
                    {

                        IsTyping = true;

                        await Task.Delay(2000);

                        IsTyping = false;

                    });
                }
            };
        }

        private void LoadChatHistory()
        {
            bool fileNeedsSync = false; // Flag to tell us if we found new messages to mark as Read

            if (System.IO.File.Exists(_currentChatFilePath))
            {
                string[] savedMessages = System.IO.File.ReadAllLines(_currentChatFilePath);

                foreach (string line in savedMessages)
                {
                    string[] parts = line.Split(new char[] { '|' }, 5);

                    if (parts.Length == 5)
                    {
                        bool isSeen = bool.Parse(parts[4]);


                        if (TargetUser != null && parts[0] == TargetUser.Username && isSeen == false)
                        {

                            isSeen = true; 

                            fileNeedsSync = true;

                            _chatService.SendReadReceipt(TargetUser.UserId, int.Parse(parts[2]));

                        }

                        Messages.Add(new MessageModel
                        {

                            Sender = parts[0],

                            Timestamp = parts[1],

                            MessageId = int.Parse(parts[2]),

                            Content = parts[3],

                            IsSent = true,

                            IsSeen = isSeen

                        });
                    }
                }
            }

            if (fileNeedsSync)
            {

                SyncChatFile();

            }
        }

        private void OnMessageReceived(byte senderId, int messageId, string messageContent, string timeString)
        {
            if (TargetUser != null && senderId == TargetUser.UserId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {

                    Messages.Add(new MessageModel
                    {

                        MessageId = messageId,

                        Sender = TargetUser.Username,

                        Timestamp = timeString,

                        Content = messageContent,

                        IsSent = true,

                        IsSeen = true 

                    });

                    SyncChatFile();

                    _chatService.SendReadReceipt(senderId, messageId);

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

            string currentTime = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");

            int generatedMessageId = new Random().Next(1, int.MaxValue);

            if (TargetGroup != null)
            {

                _chatService.SendGroupMessage(_mainViewModel.MyUserId, (byte)TargetGroup.GroupId, generatedMessageId, cipherText);

            }
            else if (TargetUser != null)
            {

                _chatService.SendMessage(_mainViewModel.MyUserId, TargetUser.UserId, cipherText, generatedMessageId);

            }

            string fileLine = $"{_mainViewModel.MyUsername}|{currentTime}|{generatedMessageId}|{InputText}\n";

            System.IO.File.AppendAllText(_currentChatFilePath, fileLine);


            Messages.Add(new MessageModel
            {

                MessageId = generatedMessageId,

                Sender = _mainViewModel.MyUsername,

                Timestamp = currentTime,

                Content = InputText,

                IsSent = false,

                IsSeen = false

            });

            InputText = string.Empty;
        }


        private void SyncChatFile()
        {

            var lines = Messages.Select(m => $"{m.Sender}|{m.Timestamp}|{m.MessageId}|{m.Content}|{m.IsSeen}");

            System.IO.File.WriteAllLines(_currentChatFilePath, lines);

        }

        private void ExecuteBack(object parameter)
        {

            _chatService.MessageReceived -= OnMessageReceived;

            _chatService.MessageStatusChanged -= OnMessageStatusChanged;

            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

        }
    }
}