using Client_Side_ChatApp.Core;
namespace Client_Side_ChatApp.ViewModels
{

    public class MainViewModel : ObservableObject
    {
        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string MyUsername { get; set; }

        public MainViewModel()
        {
            // Start the app on Layer 1
            CurrentView = new LoginViewModel(this);
        }
    }
}