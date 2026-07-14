using System.IO;
using System.Text;
using ClientSideChatApp.Core; 

namespace ClientSideChatApp.Messages
{
    public class CreateGroupRequest
    {
        private string _groupName;

        private List<byte> _userIdsToInvite;

        public CreateGroupRequest(string groupName, List<byte> userIdsToInvite)
        {

            _groupName = groupName;

            _userIdsToInvite = userIdsToInvite;

        }

        public byte GetId()
        {

            return (byte)MessageId.CREATE_GROUP;

        }

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                byte[] nameBytes = Encoding.UTF8.GetBytes(_groupName);

                writer.Write((byte)nameBytes.Length);

                writer.Write(nameBytes);

                writer.Write((byte)_userIdsToInvite.Count);

                foreach (byte userId in _userIdsToInvite)
                {

                    writer.Write(userId);

                }

                return ms.ToArray();
            }
        }
    }
}