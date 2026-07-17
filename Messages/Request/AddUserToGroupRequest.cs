using System.IO;
using ClientSideChatApp.Core;

namespace ClientSideChatApp.Messages
{
    public class AddUserToGroupRequest
    {
        private byte _groupId;

        private byte _userIdToAdd;

        public AddUserToGroupRequest(byte groupId, byte userIdToAdd)
        {

            _groupId = groupId;

            _userIdToAdd = userIdToAdd;

        }

        public byte GetId()
        {

            return (byte)MessageId.ADD_USER_TO_GROUP;

        }

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(_groupId);

                writer.Write(_userIdToAdd);

                return ms.ToArray();
            }
        }
    }
}