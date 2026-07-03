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
        public byte[] ToBytes() => new byte[] { _userId };
    }
}