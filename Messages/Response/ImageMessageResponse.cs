using System.IO;

namespace ClientSideChatApp.Messages
{
    public class ImageMessageResponse
    {
        public byte SenderId { get; private set; }
        public int Messageid { get; private set; }
        public byte[] ImageBytes { get; private set; }

        public ImageMessageResponse(byte[] payload)
        {

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {

                SenderId = reader.ReadByte();

                Messageid = reader.ReadInt32();

                ImageBytes = reader.ReadBytes((int)(ms.Length - ms.Position));

            }
        }
    }
}