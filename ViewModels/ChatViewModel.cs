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

        public string HeaderText => $"Chat with {TargetUser.Username}";

        public ObservableCollection<MessageModel> Messages { get; set; }

        private string _inputText;

        public string InputText
        {

            get { return _inputText; }

            set { _inputText = value; OnPropertyChanged(); }

        }

        public RelayCommand SendCommand { get; set; }

        public RelayCommand BackCommand { get; set; }

        public ChatViewModel(MainViewModel mainViewModel, TcpChatService chatService, UserModel targetUser)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            TargetUser = targetUser;

            Messages = new ObservableCollection<MessageModel>();

            // Setup the file paths
            string folderPath = $@"D:\ChatAppArke\ChatLogs_{_mainViewModel.MyUsername}";

            _currentChatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{TargetUser.UserId}.txt");

            System.IO.Directory.CreateDirectory(folderPath);

            // Load the Hard Drive into RAM
            if (System.IO.File.Exists(_currentChatFilePath))
            {

                string[] savedMessages = System.IO.File.ReadAllLines(_currentChatFilePath);

                foreach (string line in savedMessages)
                {

                    string[] parts = line.Split(new char[] { '|' }, 2);

                    if (parts.Length == 2)
                    {

                        Messages.Add(new MessageModel
                        {

                            Sender = parts[0],

                            Content = parts[1]

                        });
                    }
                }
            }

            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);

            BackCommand = new RelayCommand(ExecuteBack);

            _chatService.MessageReceived += OnMessageReceived;

            _chatService.FetchMissedMessages(_mainViewModel.MyUserId);

        }

        private void OnMessageReceived(byte senderId, string messageContent)
        {

            if (senderId == TargetUser.UserId)
            {

                Application.Current.Dispatcher.Invoke(() =>
                {

                    Messages.Add(new MessageModel
                    {

                        Sender = TargetUser.Username,
                        Content = messageContent

                    });
                });
            }
        }

        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(InputText);

        private void ExecuteSend(object parameter)
        {

            string cipherText = EncryptionManager.EncryptMessage(InputText);

            _chatService.SendMessage(_mainViewModel.MyUserId, TargetUser.UserId, cipherText);

            string fileLine = $"{_mainViewModel.MyUsername}|{InputText}\n";

            System.IO.File.AppendAllText(_currentChatFilePath, fileLine);

            Messages.Add(new MessageModel
            {

                Sender = _mainViewModel.MyUsername,

                Content = InputText

            });

            InputText = string.Empty;
        }

        private void ExecuteBack(object parameter)
        {

            _chatService.MessageReceived -= OnMessageReceived;

            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

        }
    }
}