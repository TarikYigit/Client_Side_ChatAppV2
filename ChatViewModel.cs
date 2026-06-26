using System.Collections.ObjectModel;
using Client_Side_ChatApp.Core;
using Client_Side_ChatApp.Models;
namespace Client_Side_ChatApp.ViewModels
{


    public class ChatViewModel : ObservableObject
    {
        private MainViewModel _mainViewModel;
        public UserModel TargetUser { get; set; }

        public string HeaderText => $"Chat with {TargetUser.Username}"; //chat interface header

        public ObservableCollection<MessageModel> Messages { get; set; }

        private string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set { _inputText = value; OnPropertyChanged(); }
        }

        public RelayCommand SendCommand { get; set; }
        public RelayCommand BackCommand { get; set; }

        public ChatViewModel(MainViewModel mainViewModel, UserModel targetUser) //make the chat with the specific person
        {
            _mainViewModel = mainViewModel;
            TargetUser = targetUser;
            Messages = new ObservableCollection<MessageModel>();

            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);  //if send is clicked, execute send
            BackCommand = new RelayCommand(ExecuteBack);
        }

        private bool CanExecuteSend(object parameter) => !string.IsNullOrWhiteSpace(InputText); //if its not empty

        private void ExecuteSend(object parameter)
        {
            Messages.Add(new MessageModel //take out when connecting server
            {
                Sender = _mainViewModel.MyUsername,
                Content = InputText
            });

            InputText = string.Empty;
        }

        private void ExecuteBack(object parameter) //return back to connection page
        {
            // Return to Layer 2
            _mainViewModel.CurrentView = new UserListViewModel(_mainViewModel);
        }
    }
}