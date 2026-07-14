using System.IO;
using System.Text;

namespace ClientSideChatApp.Messages
{
    public class GroupListResponse
    {
        public List<GroupModel> Groups { get; private set; } = new List<GroupModel>();

        public GroupListResponse(byte[] payload)
        {

            if (payload == null || payload.Length == 0) return;

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {

                byte groupCount = reader.ReadByte();

                for (int i = 0; i < groupCount; i++)
                {

                    byte groupId = reader.ReadByte();

                    byte nameLength = reader.ReadByte();

                    byte[] nameBytes = reader.ReadBytes(nameLength);

                    string groupName = Encoding.UTF8.GetString(nameBytes);

                    Groups.Add(new GroupModel { GroupId = groupId, GroupName = groupName });

                }
            }
        }
    }
}