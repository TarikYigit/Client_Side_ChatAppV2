using ClientSideChatApp.Core;
using System;
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

            string filePath = @"C:\Users\tarik.dalkiran\Desktop\user_ID_file.txt";

            if (System.IO.File.Exists(filePath))
            {

                string[] savedUsers = System.IO.File.ReadAllLines(filePath);

                foreach (string line in savedUsers)
                {

                    string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    // log in as an Existing User
                    if (parts.Length == 2 && parts[0] == UsernameInput)
                    {

                        byte savedId = byte.Parse(parts[1]);

                        _chatService.ConnectExistingUser(UsernameInput);

                        return; 

                    }
                }
            }

            _chatService.ConnectAndSetUser(UsernameInput, "127.0.0.1");
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

                MessageBox.Show("Login rejected by server. Username might be taken.", "Error");

            });
        }
    }
}