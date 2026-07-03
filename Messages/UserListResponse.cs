using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClientSideChatApp.Messages
{
    internal class UserListResponse
    {
        public Dictionary<byte, string> Users { get; private set; } = new Dictionary<byte, string>();

        public UserListResponse(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return;

            using (MemoryStream ms = new MemoryStream(payload))
            using (BinaryReader payloadReader = new BinaryReader(ms))
            {
                byte userCount = payloadReader.ReadByte();

                for (int i = 0; i < userCount; i++)
                {
                    byte userId = payloadReader.ReadByte();
                    byte nameLength = payloadReader.ReadByte();
                    byte[] nameBuffer = payloadReader.ReadBytes(nameLength);
                    string username = Encoding.UTF8.GetString(nameBuffer);

                    Users[userId] = username;
                }
            }
        }
    }
}