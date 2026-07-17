using System.Collections.ObjectModel;
using System.Linq;
using ClientSideChatApp.Core;
using ClientSideChatApp.Models;

namespace ClientSideChatApp.ViewModels
{
    public class AddUserToGroupViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;

        private TcpChatService _chatService;

        private GroupModel _targetGroup;

        public ObservableCollection<UserModel> AvailableUsers { get; set; }

        public string HeaderText => $"Add to {_targetGroup.GroupName}";

        public RelayCommand AddSelectedUsersCommand { get; set; }

        public RelayCommand BackCommand { get; set; }

        public AddUserToGroupViewModel(MainViewModel mainViewModel, TcpChatService chatService, GroupModel group, ObservableCollection<UserModel> allUsers)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            _targetGroup = group;

            AvailableUsers = new ObservableCollection<UserModel>(allUsers.Select(u => new UserModel { UserId = u.UserId, Username = u.Username, IsSelected = false }));

            AddSelectedUsersCommand = new RelayCommand(ExecuteAdd, CanExecuteAdd);

            BackCommand = new RelayCommand(ExecuteBack);
        }

        private bool CanExecuteAdd(object parameter) => AvailableUsers.Any(u => u.IsSelected);

        private void ExecuteAdd(object parameter)
        {
            var selectedUsers = AvailableUsers.Where(u => u.IsSelected);

            foreach (var user in selectedUsers)
            {

                _chatService.AddUserToGroup((byte)_targetGroup.GroupId, user.UserId);

            }

            ExecuteBack(null);
        }

        private void ExecuteBack(object parameter)
        {

            _mainViewModel.CurrentView = new GroupChatViewModel(_mainViewModel, _chatService, _targetGroup);

        }
    }
}