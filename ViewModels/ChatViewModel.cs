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

            string folderPath = $@"C:\Users\tarik.dalkiran\Desktop\Workspace\ChatLogs_{_mainViewModel.MyUsername}";

            _currentChatFilePath = System.IO.Path.Combine(folderPath, $"ChatWith_{TargetUser.UserId}.txt");

            System.IO.Directory.CreateDirectory(folderPath);

            if (System.IO.File.Exists(_currentChatFilePath))
            {

                string[] savedMessages = System.IO.File.ReadAllLines(_currentChatFilePath);

                foreach (string line in savedMessages)
                {

                    string[] parts = line.Split(new char[] { '|' }, 3);

                    if (parts.Length == 3)
                    {

                        Messages.Add(new MessageModel
                        {

                            Sender = parts[0],

                            Timestamp = parts[1], 

                            Content = parts[2]  

                        });
                    }
                }
            }

            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);

            BackCommand = new RelayCommand(ExecuteBack);

            _chatService.MessageReceived += OnMessageReceived;

            _chatService.FetchMissedMessages(_mainViewModel.MyUserId);

        }

        private void OnMessageReceived(byte senderId, string messageContent, string timeString)
        {

            if (senderId == TargetUser.UserId)
            {

                Application.Current.Dispatcher.Invoke(() =>
                {

                    Messages.Add(new MessageModel
                    {

                        Sender = TargetUser.Username,

                        Timestamp = timeString, 

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

            string currentTime = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");

            string fileLine = $"{_mainViewModel.MyUsername}|{currentTime}|{InputText}\n";

            System.IO.File.AppendAllText(_currentChatFilePath, fileLine);

            Messages.Add(new MessageModel
            {

                Sender = _mainViewModel.MyUsername,

                Timestamp = currentTime,

                Content = InputText

            }

            );

            InputText = string.Empty;
        }

        private void ExecuteBack(object parameter)
        {

            _chatService.MessageReceived -= OnMessageReceived;

            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

        }
    }
}