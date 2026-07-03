using System.IO;

namespace ClientSideChatApp.Messages
{
    internal class LoginResponse
    {

        public bool IsAccepted { get; private set; }

        public byte AssignedUserId { get; private set; }

        public LoginResponse(byte[] payload)
        {

            if (payload == null || payload.Length == 0) return;

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {

                byte status = reader.ReadByte();

                IsAccepted = (status == 0x01);

                if (IsAccepted && ms.Position < ms.Length)
                {

                    AssignedUserId = reader.ReadByte();

                }
            }
        }
    }
}