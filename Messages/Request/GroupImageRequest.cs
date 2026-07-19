using ClientSideChatApp.Core;
using System.IO;

namespace ClientSideChatApp.Messages
{
    public class GroupImageRequest
    {
        public byte SenderId { get; private set; }

        public byte GroupId { get; private set; }

        public int Messageid { get; private set; }

        public byte[] ImageBytes { get; private set; }

        public GroupImageRequest(byte senderId, byte groupId, int messageId, byte[] imageBytes)
        {

            SenderId = senderId;

            GroupId = groupId;

            Messageid = messageId;

            ImageBytes = imageBytes ?? new byte[0];

        }

        public byte GetId() => (byte)MessageId.GROUP_IMAGE;

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(SenderId);

                writer.Write(GroupId);

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