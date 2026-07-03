using System;
using System.Text;

namespace ClientSideChatApp.Messages
{
    internal class ChatMessageResponse
    {
        public byte SenderId { get; private set; }
        public string Message { get; private set; }

        public ChatMessageResponse(byte[] payload)
        {
            if (payload != null && payload.Length > 0)
            {
                SenderId = payload[0];  //get ID

                if (payload.Length > 1) //get message if it exists
                {
                    byte[] msgBuffer = new byte[payload.Length - 1];
                    Array.Copy(payload, 1, msgBuffer, 0, msgBuffer.Length);
                    Message = Encoding.UTF8.GetString(msgBuffer);
                }
                else
                {
                    Message = string.Empty;
                }
            }
        }
    }
}