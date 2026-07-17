using System;
using System.Collections.ObjectModel;
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

            _chatService.FetchMissedMessages(_mainViewModel.MyUserId);

        }

        private void LoadChatHistory()
        {
            if (System.IO.File.Exists(_currentChatFilePath))
            {
                string[] savedMessages = System.IO.File.ReadAllLines(_currentChatFilePath);

                foreach (string line in savedMessages)
                {
                    string[] parts = line.Split(new char[] { '|' }, 3);

                    if (parts.Length == 3)
                    {
                        Messages.Add(new MessageModel { Sender = parts[0], Timestamp = parts[1], Content = parts[2] });
                    }
                }
            }
        }

        private void OnGroupMessageReceived(byte groupId, string senderName, string messageContent, string timeString)
        {
            if (groupId == TargetGroup.GroupId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new MessageModel { Sender = senderName, Timestamp = timeString, Content = messageContent });
                });
            }
        }

        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(InputText);

        private void ExecuteSend(object parameter)
        {

            string cipherText = EncryptionManager.EncryptMessage(InputText);

            string currentTime = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");

            _chatService.SendGroupMessage(_mainViewModel.MyUserId, (byte)TargetGroup.GroupId, cipherText);

            string fileLine = $"{_mainViewModel.MyUsername}|{currentTime}|{InputText}\n";

            System.IO.File.AppendAllText(_currentChatFilePath, fileLine);

            Messages.Add(new MessageModel { Sender = _mainViewModel.MyUsername, Timestamp = currentTime, Content = InputText });

            InputText = string.Empty;

        }

        private void ExecuteAddUser(object parameter)
        {
            var availableUsers = new ObservableCollection<UserModel>(
                _chatService.AllUsers.Where(u => u.UserId != _mainViewModel.MyUserId)
            );

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

            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

        }
    }
}