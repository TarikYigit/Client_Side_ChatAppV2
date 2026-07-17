using System.IO;
using ClientSideChatApp.Core;

namespace ClientSideChatApp.Messages
{
    public class LeaveGroupRequest
    {
        private byte _userId;

        private byte _groupId;

        public LeaveGroupRequest(byte userId, byte groupId)
        {

            _userId = userId;

            _groupId = groupId;

        }

        public byte GetId()
        {

            return (byte)MessageId.LEAVE_GROUP;

        }

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(_userId);

                writer.Write(_groupId);

                return ms.ToArray();
            }
        }
    }
}