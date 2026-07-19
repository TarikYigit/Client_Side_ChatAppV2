using ClientSideChatApp.Core;
using System.IO;

namespace ClientSideChatApp.Messages
{
    public class SendImageRequest
    {
        public byte SenderId { get; private set; }
        public byte ReceiverId { get; private set; }
        public int Messageid { get; private set; }
        public byte[] ImageBytes { get; private set; }

        public SendImageRequest(byte senderId, byte receiverId, int messageId, byte[] imageBytes)
        {

            SenderId = senderId;

            ReceiverId = receiverId;

            Messageid = messageId;

            ImageBytes = imageBytes ?? new byte[0];

        }

        public byte GetId() => (byte)MessageId.SEND_IMAGE;

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(SenderId);

                writer.Write(ReceiverId);

                writer.Write(Messageid);

                if (ImageBytes.Length > 0)
                {

                    writer.Write(ImageBytes);

                }

                return ms.ToArray();
            }
        }
    }
}