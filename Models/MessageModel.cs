using ClientSideChatApp.Core;


namespace ClientSideChatApp.Models
{

    public class MessageModel : ObservableObject
    {

        public string Sender { get; set; }

        public string Content { get; set; }

        public string Reciever { get; set; }

        public string Timestamp { get; set; }

        private bool _isSent;
        public bool IsSent
        {
            get { return _isSent; }
            set { _isSent = value; OnPropertyChanged(); } // Alerts XAML to show ✓✓
        }

        private bool _isSeen;
        public bool IsSeen
        {
            get { return _isSeen; }
            set { _isSeen = value; OnPropertyChanged(); } // Alerts XAML to turn ticks Blue
        }

        public int MessageId { get; set; }

    }
}