//Holding username of the user (might delete)
using ClientSideChatApp.Core;
namespace ClientSideChatApp.Models
{

    public class UserModel
    {
        public string Username { get; set; }

        public byte UserId { get; set; }

        public bool IsOnline { get; set; }

    }
}