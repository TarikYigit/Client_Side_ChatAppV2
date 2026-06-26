using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ArketelChat
{
    // --- 1. MVVM BASE CLASSES ---
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    // --- 2. VIEW MODELS ---
    public class MainViewModel : ObservableObject
    {
        private object _currentViewModel;
        public string LocalUserName { get; set; }

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            // Start at Layer 1
            CurrentViewModel = new LoginViewModel(this);
        }

        // Navigation Methods
        public void NavigateToUserSelection()
        {
            CurrentViewModel = new UserSelectionViewModel(this);
        }

        public void NavigateToChat(string chatPartner)
        {
            CurrentViewModel = new ChatViewModel(this, chatPartner);
        }
    }

    // Layer 1: Login
    public class LoginViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private string _userName;

        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object obj) => !string.IsNullOrWhiteSpace(UserName);

        private void ExecuteLogin(object obj)
        {
            _mainViewModel.LocalUserName = UserName;

            // TODO: Initialize TCP Client here and register the username with the server.
            // _tcpService.ConnectAndRegister(UserName);

            _mainViewModel.NavigateToUserSelection();
        }
    }

    // Layer 2: User Selection
    public class UserSelectionViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private string _selectedUser;

        // ObservableCollection allows the list to update dynamically as more than 2 users connect
        public ObservableCollection<string> AvailableUsers { get; set; }

        public string SelectedUser
        {
            get => _selectedUser;
            set { _selectedUser = value; OnPropertyChanged(); }
        }

        public ICommand StartChatCommand { get; }

        public UserSelectionViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            StartChatCommand = new RelayCommand(StartChat, CanStartChat);

            // Mock Data - In reality, this list comes from your TCP Server
            AvailableUsers = new ObservableCollection<string>
            {
                "Alice",
                "Bob",
                "Charlie",
                "DevOps_Node_4" // Proves it scales past 3
            };
        }

        private bool CanStartChat(object obj) => !string.IsNullOrEmpty(SelectedUser);

        private void StartChat(object obj)
        {
            _mainViewModel.NavigateToChat(SelectedUser);
        }
    }

    // Layer 3: Chat Interface
    public class ChatViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private string _currentMessage;

        public string ChatPartner { get; }
        public ObservableCollection<string> Messages { get; set; }

        public string CurrentMessage
        {
            get => _currentMessage;
            set { _currentMessage = value; OnPropertyChanged(); }
        }

        public ICommand SendMessageCommand { get; }
        public ICommand GoBackCommand { get; }

        public ChatViewModel(MainViewModel mainViewModel, string chatPartner)
        {
            _mainViewModel = mainViewModel;
            ChatPartner = chatPartner;
            Messages = new ObservableCollection<string>();

            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
            GoBackCommand = new RelayCommand(GoBack);

            // Hook up your TCP listener event here to receive messages
            // _tcpService.OnMessageReceived += HandleIncomingMessage;
        }

        private bool CanSendMessage(object obj) => !string.IsNullOrWhiteSpace(CurrentMessage);

        private void SendMessage(object obj)
        {
            string formattedMessage = $"[{_mainViewModel.LocalUserName}]: {CurrentMessage}";
            Messages.Add(formattedMessage);

            // TODO: Send over TCP
            // _tcpService.SendMessage(ChatPartner, CurrentMessage);

            CurrentMessage = string.Empty; // Clear textbox
        }

        private void GoBack(object obj)
        {
            _mainViewModel.NavigateToUserSelection();
        }
    }
}