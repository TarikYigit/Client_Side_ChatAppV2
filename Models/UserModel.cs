namespace ClientSideChatApp.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public byte UserId { get; set; }

        public bool IsSelected { get; set; }
        public bool IsOnline { get; set; }
    }
}