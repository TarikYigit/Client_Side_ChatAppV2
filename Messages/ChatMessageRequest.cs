using ClientSideChatApp.Core;
using System.IO;
using System.Text;

namespace ClientSideChatApp.Messages
{

    internal class ChatMessageRequest
    {

        public byte SenderId { get; private set; }

        public byte ReceiverId { get; private set; }

        public string Content { get; private set; }

        public ChatMessageRequest(byte senderId, byte receiverId, string content)
        {

            SenderId = senderId;

            ReceiverId = receiverId;

            Content = content;

        }

        public byte GetId() => (byte)MessageId.CHAT_MESSAGE;

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(SenderId);

                writer.Write(ReceiverId);

                writer.Write(Encoding.UTF8.GetBytes(Content));

                return ms.ToArray();

            }
        }
    }
}