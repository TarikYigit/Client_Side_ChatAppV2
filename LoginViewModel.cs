using Client_Side_ChatApp.Core;
namespace Client_Side_ChatApp.ViewModels
{


    public class LoginViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        private string _usernameInput;
        public string UsernameInput
        {
            get { return _usernameInput; }
            set { _usernameInput = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; set; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter) => !string.IsNullOrWhiteSpace(UsernameInput);
        //Take username from view to viewmodel 
        private void ExecuteLogin(object parameter)
        {
            _mainViewModel.MyUsername = UsernameInput;
            // Go to Layer viewmodel here
            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel);
        }
    }
}