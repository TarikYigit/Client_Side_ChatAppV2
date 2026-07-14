using System.IO;
using System.Text;
using ClientSideChatApp.Core;

namespace ClientSideChatApp.Messages
{
    public class GroupChatMessageRequest
    {

        private byte _senderId;

        private byte _groupId;

        private string _content;

        public GroupChatMessageRequest(byte senderId, byte groupId, string content)
        {

            _senderId = senderId;

            _groupId = groupId;

            _content = content;

        }

        public byte GetId()
        {

            return (byte)MessageId.GROUP_CHAT_MESSAGE;

        }

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(_senderId);

                writer.Write(_groupId);

                byte[] contentBytes = Encoding.UTF8.GetBytes(_content);

                writer.Write(contentBytes);

                return ms.ToArray();

            }
        }
    }
}