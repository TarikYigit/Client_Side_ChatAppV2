using ClientSideChatApp.Core;

namespace ClientSideChatApp.Models
{
    public class MessageModel : ObservableObject
    {
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Timestamp { get; set; }
        public int MessageId { get; set; }
        public bool IsMyMessage { get; set; }

        private string _content;
        public string Content
        {
            get { return _content; } 
            set
            {
                _content = value;

                if (_content != null && _content.StartsWith("[IMG:") && _content.EndsWith("]"))
                {

                    ImagePath = _content.Substring(5, _content.Length - 6);

                }

                OnPropertyChanged();

                OnPropertyChanged(nameof(DisplayContent)); 
            }
        }

        public string DisplayContent
        {
            get
            {

                if (_content != null && _content.StartsWith("[IMG:")) return "🖼️ Photo";

                return _content;
            }
        }

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

        private string _imagePath;
        public string ImagePath
        {

            get { return _imagePath; }
            set
            {

                _imagePath = value;

                OnPropertyChanged();

                OnPropertyChanged(nameof(HasImage)); 

            }
        }

        public bool HasImage => !string.IsNullOrEmpty(ImagePath);


    }
}