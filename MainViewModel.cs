using ClientSideChatApp.Core;

namespace ClientSideChatApp.ViewModels
{

    public class MainViewModel : ObservableObject
    {

        private TcpChatService _masterChatService;

        private object _currentView;

        public object CurrentView
        {

            get { return _currentView; }

            set { _currentView = value; OnPropertyChanged(); }

        }

        public string MyUsername { get; set; }

        public byte MyUserId { get; set; }

        public MainViewModel()
        {

            _masterChatService = new TcpChatService();

            CurrentView = new LoginViewModel(this, _masterChatService);

        }
    }
}