using ClientSideChatApp.Core;
using ClientSideChatApp.Core;
using System.Text;

namespace ClientSideChatApp.Messages
{
    internal class LoginRequest
    {
        public string Username { get; private set; }
        public LoginRequest(string username) { Username = username; }

        public byte GetId() => (byte)MessageId.LOG_IN; 
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(Username);
    }
}