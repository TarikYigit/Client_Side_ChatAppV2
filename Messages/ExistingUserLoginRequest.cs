using ClientSideChatApp.Core;
using ClientSideChatApp.Core;
using System.Text;

namespace ClientSideChatApp.Messages
{
    internal class ExistingUserLoginRequest
    {
        public string Username { get; private set; }
        public ExistingUserLoginRequest(string username) { Username = username; }

        public byte GetId() => (byte)MessageId.EXISTING_USER_LOG_IN; 
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(Username);
    }
}