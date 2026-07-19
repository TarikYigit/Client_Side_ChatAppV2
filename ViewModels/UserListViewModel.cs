using ClientSideChatApp.Core;
using ClientSideChatApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace ClientSideChatApp.ViewModels
{
    public class UserListViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        private TcpChatService _chatService;
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<GroupModel> Groups { get; set; }

        private UserModel _selectedUser;

        private string _searchText = string.Empty;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;

                OnPropertyChanged();

                CollectionViewSource.GetDefaultView(Users).Refresh();

                CollectionViewSource.GetDefaultView(Groups).Refresh();

            }
        }

        public UserModel SelectedUser
        {
            get { return _selectedUser; }

            set
            {
                _selectedUser = value;

                if (_selectedUser != null)
                {

                    SelectedGroup = null;

                }
                OnPropertyChanged();
            }
        }



        private GroupModel _selectedGroup;
        public GroupModel SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;

                if (_selectedGroup != null)
                {

                    SelectedUser = null;

                }
                OnPropertyChanged();
            }
        }

        private string _groupName;
        public string GroupName
        {

            get { return _groupName; }

            set { _groupName = value; OnPropertyChanged(); }

        }

        public RelayCommand ConnectCommand { get; set; }

        public RelayCommand OpenCreateGroupCommand { get; set; } 

        public UserListViewModel(MainViewModel mainViewModel, TcpChatService chatService)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;


            Users = new ObservableCollection<UserModel>();

            Groups = new ObservableCollection<GroupModel>();

            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);

            OpenCreateGroupCommand = new RelayCommand(ExecuteOpenCreateGroup);

            _chatService.UserListUpdated += OnUserListUpdated;

            _chatService.RequestClientList(_mainViewModel.MyUserId);

            _chatService.GroupListUpdated += OnGroupListUpdated;

            _chatService.RequestGroupList(_mainViewModel.MyUserId);

            ICollectionView userView = CollectionViewSource.GetDefaultView(Users);

            userView.Filter = o => string.IsNullOrWhiteSpace(SearchText) || ((UserModel)o).Username.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

            ICollectionView groupView = CollectionViewSource.GetDefaultView(Groups);

            groupView.Filter = o => string.IsNullOrWhiteSpace(SearchText) || ((GroupModel)o).GroupName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
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

        private void OnGroupListUpdated(List<GroupModel> updatedGroups)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                Groups.Clear();

                foreach (GroupModel group in updatedGroups)
                {

                    Groups.Add(group);

                }
            });
        }


        private bool CanExecuteConnect(object parameter)
        {

            return SelectedUser != null || SelectedGroup != null;

        }

        private void ExecuteConnect(object parameter)
        {

            if (SelectedUser != null)
            {

                SelectedUser.UnreadCount = 0; 

                _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, _chatService, SelectedUser);

            }
            else if (SelectedGroup != null)
            {

                SelectedGroup.UnreadCount = 0; 

                _mainViewModel.CurrentView = new GroupChatViewModel(_mainViewModel, _chatService, SelectedGroup);

            }
        }


        private void ExecuteOpenCreateGroup(object parameter)
        {

            _mainViewModel.CurrentView = new CreateGroupViewModel(_mainViewModel, _chatService, Users);

        }
    }
}