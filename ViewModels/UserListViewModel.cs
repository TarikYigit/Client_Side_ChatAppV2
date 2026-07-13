using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows; // Required for Application.Current.Dispatcher
using ClientSideChatApp.Models;
using ClientSideChatApp.Core;


namespace ClientSideChatApp.ViewModels
{

    public class UserListViewModel : ObservableObject
    {

        private MainViewModel _mainViewModel;

        private TcpChatService _chatService;

        public ObservableCollection<UserModel> Users { get; set; }

        private UserModel _selectedUser;

        public UserModel SelectedUser
        {

            get { return _selectedUser; }

            set
            {
                _selectedUser = value;

                OnPropertyChanged();

            }
        }

        public RelayCommand ConnectCommand { get; set; }



        public UserListViewModel(MainViewModel mainViewModel, TcpChatService chatService)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            Users = new ObservableCollection<UserModel>();

            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);

            _chatService.UserListUpdated += OnUserListUpdated;

            _chatService.RequestClientList(_mainViewModel.MyUserId);

        }

        private void OnUserListUpdated(List<UserModel> updatedUsers)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear(); 

                foreach (UserModel user in updatedUsers)
                {

                    if (user.UserId == _mainViewModel.MyUserId) continue;

                    Users.Add(user);
                }
            });
        }



        private bool CanExecuteConnect(object parameter) => SelectedUser != null;

        private void ExecuteConnect(object parameter)
        {

            _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, _chatService, SelectedUser);

        }
    }
}