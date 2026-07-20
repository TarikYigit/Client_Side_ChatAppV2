using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ClientSideChatApp
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

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

    public class MainViewModel : ObservableObject
    {

        private object _currentViewModel;

        public string LocalUserName { get; set; }

        public string LocalPassword { get; set; }

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {

            CurrentViewModel = new LoginViewModel(this);

        }

        public void NavigateToUserSelection()
        {

            CurrentViewModel = new UserSelectionViewModel(this);

        }

        public void NavigateToChat(string chatPartner)
        {

            CurrentViewModel = new ChatViewModel(this, chatPartner);

        }


    }

    //  Login
    public class LoginViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        private string _userName;

        private string _password;

        public string UserName
        {

            get => _userName;

            set { _userName = value; OnPropertyChanged(); }

        }
        public string Password
        {

            get => _password;

            set { _password = value; OnPropertyChanged(); }

        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel)
        {

            _mainViewModel = mainViewModel;

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteAuth);

            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteAuth);

        }

        private bool CanExecuteAuth(object obj) => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

        private void ExecuteLogin(object obj)
        {

            _mainViewModel.LocalUserName = UserName;

            _mainViewModel.LocalPassword = Password;

            _mainViewModel.NavigateToUserSelection();

        }

        private void ExecuteRegister(object obj)
        {

            _mainViewModel.LocalUserName = UserName;

            _mainViewModel.LocalPassword = Password;

            _mainViewModel.NavigateToUserSelection();

        }
    }

    //User Selection
    public class UserSelectionViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        private string _selectedUser;

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

        }

        private bool CanStartChat(object obj) => !string.IsNullOrEmpty(SelectedUser);

        private void StartChat(object obj)
        {

            _mainViewModel.NavigateToChat(SelectedUser);

        }
    }

    //Chat Interface
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

        }

        private bool CanSendMessage(object obj) => !string.IsNullOrWhiteSpace(CurrentMessage);

        private void SendMessage(object obj)
        {

            string formattedMessage = $"[{_mainViewModel.LocalUserName}]: {CurrentMessage}";

            Messages.Add(formattedMessage);

            CurrentMessage = string.Empty; 

        }

        private void GoBack(object obj)
        {

            _mainViewModel.NavigateToUserSelection();

        }
    }
}