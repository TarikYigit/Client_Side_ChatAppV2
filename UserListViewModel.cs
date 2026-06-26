using System.Collections.ObjectModel;
using Client_Side_ChatApp.Models;
using Client_Side_ChatApp.Core;
namespace Client_Side_ChatApp.ViewModels
{

    public class UserListViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        public ObservableCollection<UserModel> Users { get; set; }

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; OnPropertyChanged(); }
        }

        public RelayCommand ConnectCommand { get; set; }

        public UserListViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);

            // Mock UI Data (Easily expandable beyond 3 people)
            Users = new ObservableCollection<UserModel>
            {
                new UserModel { Username = "Alice" },
                new UserModel { Username = "Bob" },
                new UserModel { Username = "Charlie" },
                new UserModel { Username = "David" }
            };
        }

        private bool CanExecuteConnect(object parameter) => SelectedUser != null;

        private void ExecuteConnect(object parameter)
        {
            // Go to Layer 3
            _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, SelectedUser);
        }
    }
}