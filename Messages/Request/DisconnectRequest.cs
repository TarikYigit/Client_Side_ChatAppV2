using ClientSideChatApp.Core;

namespace ClientSideChatApp.Messages
{

    internal class DisconnectRequest
    {

        public byte GetId() => 0x04; 

        public byte[] ToBytes() => new byte[0]; //payload is empty for disconnect request

    }
}