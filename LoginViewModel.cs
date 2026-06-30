using Client_Side_ChatApp.Core;
using System;
using System.Windows; 

namespace Client_Side_ChatApp.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;
        private TcpChatService _chatService;

        private string _usernameInput;
        public string UsernameInput
        {
            get { return _usernameInput; }
            set { _usernameInput = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; set; }

        public LoginViewModel(MainViewModel mainViewModel, TcpChatService chatService)
        {
            _mainViewModel = mainViewModel;
            _chatService = chatService;

            // Subscribe to the network events
            _chatService.LoginSuccessful += OnLoginSuccessful;
            _chatService.LoginRejected += OnLoginRejected;

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter) => !string.IsNullOrWhiteSpace(UsernameInput);

        private void ExecuteLogin(object parameter)
        {

            _chatService.ConnectAndSetUser(UsernameInput, "127.0.0.1");
        }

        private void OnLoginSuccessful(byte assignedId)
        {
  
            Application.Current.Dispatcher.Invoke(() =>
            {

                _mainViewModel.MyUsername = UsernameInput;

                _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);
            });
        }

        private void OnLoginRejected()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Login rejected by server. Username might be taken.", "Error");
            });
        }
    }
}