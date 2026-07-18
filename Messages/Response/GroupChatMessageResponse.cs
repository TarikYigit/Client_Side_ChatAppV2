using System.IO;
using System.Text;

namespace ClientSideChatApp.Messages
{
    public class GroupChatMessageResponse
    {
        public byte SenderId { get; private set; }
        public byte GroupId { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Message { get; private set; }

        public int messageId { get; private set; }
        public GroupChatMessageResponse(byte[] payload)
        {

            if (payload == null || payload.Length == 0) return;

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {

                SenderId = reader.ReadByte();

                GroupId = reader.ReadByte();

                messageId = reader.ReadInt32();

                long ticks = reader.ReadInt64();

                TimeStamp = new DateTime(ticks);

                int remainingBytes = (int)(ms.Length - ms.Position);

                if (remainingBytes > 0)
                {

                    byte[] msgBytes = reader.ReadBytes(remainingBytes);

                    Message = Encoding.UTF8.GetString(msgBytes);

                }
            }
        }
    }
}