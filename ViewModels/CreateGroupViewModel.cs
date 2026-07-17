using System.Collections.ObjectModel;
using System.Linq;
using ClientSideChatApp.Core;
using ClientSideChatApp.Models;

namespace ClientSideChatApp.ViewModels
{
    public class CreateGroupViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;
        private TcpChatService _chatService;

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserModel> AvailableUsers { get; set; }

        public RelayCommand ExecuteCreateGroupCommand { get; set; }
        public RelayCommand BackCommand { get; set; }

        public CreateGroupViewModel(MainViewModel mainViewModel, TcpChatService chatService, ObservableCollection<UserModel> users)
        {

            _mainViewModel = mainViewModel;

            _chatService = chatService;

            AvailableUsers = users;

            ExecuteCreateGroupCommand = new RelayCommand(ExecuteCreate, CanExecuteCreate);

            BackCommand = new RelayCommand(ExecuteBack);
        }

        private bool CanExecuteCreate(object parameter)
        {
            return !string.IsNullOrWhiteSpace(GroupName) && AvailableUsers.Any(u => u.IsSelected);
        }

        private void ExecuteCreate(object parameter)
        {
            var selectedUserIds = AvailableUsers.Where(u => u.IsSelected).Select(u => u.UserId).ToList();

            _chatService.CreateGroup(GroupName, selectedUserIds);

            ExecuteBack(null);
        }

        private void ExecuteBack(object parameter)
        {

            foreach (var user in AvailableUsers) user.IsSelected = false;

            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel, _chatService);

        }
    }
}