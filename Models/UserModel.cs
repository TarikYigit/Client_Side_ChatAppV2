using ClientSideChatApp.Core;


namespace ClientSideChatApp.Models
{
    public class UserModel : ObservableObject
    {
        public string Username { get; set; }
        public byte UserId { get; set; }

        public bool IsSelected { get; set; }
        public bool IsOnline { get; set; }

        private int _unreadCount;
        public int UnreadCount
        {
            get { return _unreadCount; }
            set
            {

                _unreadCount = value;

                OnPropertyChanged();

                OnPropertyChanged(nameof(HasUnreadMessages));

            }
        }

        public bool HasUnreadMessages => UnreadCount > 0;
    }
}