using System.Collections.ObjectModel;
using System.Windows; // Required for Application.Current.Dispatcher
using Client_Side_ChatApp.Core;
using Client_Side_ChatApp.Models;

namespace Client_Side_ChatApp.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        // 1. Declare the chat service field
        private TcpChatService _chatService;

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

            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);
            BackCommand = new RelayCommand(ExecuteBack);
            _chatService.MessageReceived += OnMessageReceived;
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
            _chatService.SendMessage(_mainViewModel.MyUserId, TargetUser.UserId, InputText);

            Messages.Add(new MessageModel
            {
                Sender = _mainViewModel.MyUsername, // Show our name on the right side
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