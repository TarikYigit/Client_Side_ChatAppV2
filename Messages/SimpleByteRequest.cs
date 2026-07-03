using System.IO;
using ClientSideChatApp.Core;

namespace ClientSideChatApp.Messages
{

    internal class SimpleByteRequest
    {

        private byte _messageId;

        private byte _userId;

        public SimpleByteRequest(byte messageId, byte userId)
        {

            _messageId = messageId;

            _userId = userId;

        }

        public byte GetId() => _messageId;

        public byte[] ToBytes()
        {

            using (MemoryStream ms = new MemoryStream())

            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                writer.Write(_userId);

                return ms.ToArray();

            }
        }
    }
}