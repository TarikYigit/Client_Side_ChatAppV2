using System.IO;
using System.Text;
using ClientSideChatApp.Models; 

namespace ClientSideChatApp.Messages
{
    internal class UserListResponse
    {
        public List<UserModel> Users { get; private set; } = new List<UserModel>();

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

                    bool isOnline = payloadReader.ReadBoolean();

                    byte nameLength = payloadReader.ReadByte();
                    byte[] nameBuffer = payloadReader.ReadBytes(nameLength);
                    string username = Encoding.UTF8.GetString(nameBuffer);

                    Users.Add(new UserModel
                    {

                        UserId = userId,

                        Username = username,

                        IsOnline = isOnline

                    });
                }
            }
        }
    }
}