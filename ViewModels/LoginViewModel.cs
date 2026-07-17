using ClientSideChatApp.Core;
using System.Windows;

namespace ClientSideChatApp.ViewModels
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

        private string _passwordInput;
        public string PasswordInput
        {

            get { return _passwordInput; }

            set { _passwordInput = value; OnPropertyChanged(); }

        }
        public RelayCommand LoginCommand { get; set; }

        public RelayCommand RegisterCommand { get; set; }

        public LoginViewModel(MainViewModel mainViewModel, TcpChatService chatService)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            _chatService.LoginSuccessful += OnLoginSuccessful;

            _chatService.LoginRejected += OnLoginRejected;

            _chatService.RegisterRejectedPassword += OnRegisterRejectedPassword;

            _chatService.RegisterRejectedUsername += OnRegisterRejectedPassword;


            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteAuth);

            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteAuth);


        }

        private bool CanExecuteAuth(object parameter) => !string.IsNullOrWhiteSpace(UsernameInput) && !string.IsNullOrWhiteSpace(PasswordInput);

        private void ExecuteLogin(object parameter)
        {

            _chatService.ConnectAndLogin(UsernameInput, PasswordInput);

        }

        private void ExecuteRegister(object parameter) 
        {

            _chatService.ConnectAndRegister(UsernameInput, PasswordInput);


        }

        private void OnLoginSuccessful(byte assignedId)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                _mainViewModel.MyUsername = UsernameInput;

                _mainViewModel.MyUserId = assignedId;

                _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

            });
        }

        private void OnLoginRejected()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                MessageBox.Show("Login rejected by server. Username or Password is incorrect", "Error");

            });
        }

        private void OnRegisterRejectedPassword()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                MessageBox.Show("Password too weak. \n Have a least: \n 1 uppercase \n 1 lower case \n 1 number \n 1 special character \n Be 8 or longer in length", "Error");

            });
        }

        private void OnRegisterRejectedUsername()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                MessageBox.Show("Register rejected by server. Username Taken.", "Error");

            });
        }
    }
}