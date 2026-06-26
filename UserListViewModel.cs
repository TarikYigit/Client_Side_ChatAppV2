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
            //dictionary here
            Users = new ObservableCollection<UserModel>
            {
                new UserModel { Username = "A" },
                new UserModel { Username = "B" },
                new UserModel { Username = "C" },
                new UserModel { Username = "D" }
            };
        }

        private bool CanExecuteConnect(object parameter) => SelectedUser != null;

        private void ExecuteConnect(object parameter)
        {
            // go to VM
            _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, SelectedUser);
        }
    }
}