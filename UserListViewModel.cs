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

        private void OnUserListUpdated(Dictionary<byte, string> updatedUsers)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear(); // Wipe the old list clean

                foreach (var kvp in updatedUsers)
                {
                    if (kvp.Key == _mainViewModel.MyUserId) continue;

                    Users.Add(new UserModel
                    {
                        UserId = kvp.Key,
                        Username = kvp.Value
                    });
                }
            });
        }

        private bool CanExecuteConnect(object parameter) => SelectedUser != null;

        private void ExecuteConnect(object parameter)
        {
            // Go to Chat View, passing the service and the person we want to talk to
            _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, _chatService, SelectedUser);
        }
    }
}