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
            set { _isSent = value; OnPropertyChanged(); } 
        }

        private bool _isSeen;
        public bool IsSeen
        {
            get { return _isSeen; }
            set { _isSeen = value; OnPropertyChanged(); } 
        }

        public int MessageId { get; set; }

        public bool IsMyMessage { get; set; }

        private string _content;
        public string newContent
        {
            get { return _content; }
            set { _content = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get { return _isEditing; }
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private bool _isEdited;
        public bool IsEdited
        {
            get { return _isEdited; }
            set { _isEdited = value; OnPropertyChanged(); }
        }

    }
}