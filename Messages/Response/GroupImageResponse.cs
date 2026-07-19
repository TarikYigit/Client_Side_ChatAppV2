using System.IO;

namespace ClientSideChatApp.Messages
{
    public class GroupImageMessageResponse
    {
        public byte SenderId { get; private set; }
        public byte GroupId { get; private set; }
        public int Messageid { get; private set; }
        public byte[] ImageBytes { get; private set; }

        public GroupImageMessageResponse(byte[] payload)
        {

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {

                SenderId = reader.ReadByte();

                GroupId = reader.ReadByte();

                Messageid = reader.ReadInt32();

                ImageBytes = reader.ReadBytes((int)(ms.Length - ms.Position));

            }
        }
    }
}