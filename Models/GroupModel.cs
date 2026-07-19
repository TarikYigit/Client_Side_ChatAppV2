
using ClientSideChatApp.Core;

public class GroupModel : ObservableObject
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }

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