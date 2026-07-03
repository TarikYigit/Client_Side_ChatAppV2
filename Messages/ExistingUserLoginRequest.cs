using System.IO;
using System.Text;
using ClientSideChatApp.Core;


namespace ClientSideChatApp.Messages
{

    internal class ExistingUserLoginRequest
    {

        public string Username { get; private set; }

        public ExistingUserLoginRequest(string username) { Username = username; }

        public byte GetId() => (byte)MessageId.EXISTING_USER_LOG_IN;

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(Encoding.UTF8.GetBytes(Username));

                return ms.ToArray();

            }
        }
    }
}