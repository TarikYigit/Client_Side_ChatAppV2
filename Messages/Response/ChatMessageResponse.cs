using System.IO;
using System.Text;

namespace ClientSideChatApp.Messages
{
    internal class ChatMessageResponse
    {

        public byte SenderId { get; private set; }

        public string Message { get; private set; }

        public ChatMessageResponse(byte[] payload)
        {

            if (payload == null || payload.Length == 0) return;

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {

                SenderId = reader.ReadByte();

                int remainingBytes = (int)(ms.Length - ms.Position);

                if (remainingBytes > 0)
                {

                    byte[] msgBytes = reader.ReadBytes(remainingBytes);

                    Message = Encoding.UTF8.GetString(msgBytes);

                }
                else
                {

                    Message = string.Empty;

                }
            }
        }
    }
}