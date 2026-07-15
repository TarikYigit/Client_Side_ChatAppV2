using ClientSideChatApp.Core;
using ClientSideChatApp.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ClientSideChatApp.ViewModels
{
    public class UserListViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        private TcpChatService _chatService;
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<GroupModel> Groups { get; set; }

        private UserModel _selectedUser;

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
        public RelayCommand CreateGroupCommand { get; set; }
        public RelayCommand LeaveGroupCommand { get; set; } 

        public UserListViewModel(MainViewModel mainViewModel, TcpChatService chatService)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;


            Users = new ObservableCollection<UserModel>();

            Groups = new ObservableCollection<GroupModel>();


            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);

            CreateGroupCommand = new RelayCommand(ExecuteCreateGroup, CanExecuteCreateGroup);

            LeaveGroupCommand = new RelayCommand(ExecuteLeaveGroup, CanExecuteLeaveGroup);


            _chatService.UserListUpdated += OnUserListUpdated;

            _chatService.RequestClientList(_mainViewModel.MyUserId);

            _chatService.GroupListUpdated += OnGroupListUpdated;

            _chatService.RequestGroupList(_mainViewModel.MyUserId);
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

                _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, _chatService, SelectedUser);

            }
            else if (SelectedGroup != null)
            {

                _mainViewModel.CurrentView = new ChatViewModel(_mainViewModel, _chatService, SelectedGroup);

            }
        }

        private bool CanExecuteCreateGroup(object parameter)
        {

            return !string.IsNullOrWhiteSpace(GroupName) && Users.Any(u => u.IsSelected);

        }

        private void ExecuteCreateGroup(object parameter)
        {

            List<byte> selectedIds = Users.Where(u => u.IsSelected)
                                          .Select(u => u.UserId)
                                          .ToList();

            _chatService.CreateGroup(GroupName, selectedIds);

            GroupName = string.Empty;

            foreach (var user in Users)
            {

                user.IsSelected = false;

            }
        }

        private bool CanExecuteLeaveGroup(object parameter)
        {

            return SelectedGroup != null;

        }

        private void ExecuteLeaveGroup(object parameter)
        {

            _chatService.LeaveGroup(_mainViewModel.MyUserId, (byte)SelectedGroup.GroupId);

            SelectedGroup = null;

        }
    }
}