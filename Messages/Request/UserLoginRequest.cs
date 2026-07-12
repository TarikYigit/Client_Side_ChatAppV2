using System.IO;
using System.Text;
using ClientSideChatApp.Core;


namespace ClientSideChatApp.Messages
{

    internal class UserLoginRequest
    {

        public string Username { get; private set; }

        public string Password { get; private set; }

        public UserLoginRequest(string username, string password) 
        { 

            Username = username; 
        
            Password = password;

        }

        public byte GetId() => (byte)MessageId.LOG_IN;

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                byte[] userBytes = Encoding.UTF8.GetBytes(Username);

                byte[] passBytes = Encoding.UTF8.GetBytes(Password);

                writer.Write((byte)userBytes.Length);

                writer.Write(userBytes);

                writer.Write((byte)passBytes.Length);

                writer.Write(passBytes);

                return ms.ToArray();

            }
        }
    }
}