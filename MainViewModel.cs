using Client_Side_ChatApp.Core;

namespace Client_Side_ChatApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // 1. Declare the master instance of your network service
        private TcpChatService _masterChatService;

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string MyUsername { get; set; }

        // 2. Add this so other ViewModels know the active user's ID
        public byte MyUserId { get; set; }

        public MainViewModel()
        {
            // 3. Create the network service the moment the app opens
            _masterChatService = new TcpChatService();

            // 4. Start the app on the Login view, injecting BOTH required parameters
            CurrentView = new LoginViewModel(this, _masterChatService);
        }
    }
}