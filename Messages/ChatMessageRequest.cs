using ClientSideChatApp.Core; // So it can see your MessageId enum
using System;
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

        public byte GetId()
        {
            return (byte)MessageId.CHAT_MESSAGE;
        }

        public byte[] ToBytes()
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(Content);
            byte[] payload = new byte[2 + contentBytes.Length];

            payload[0] = SenderId;
            payload[1] = ReceiverId;
            Array.Copy(contentBytes, 0, payload, 2, contentBytes.Length);

            return payload;
        }
    }
}