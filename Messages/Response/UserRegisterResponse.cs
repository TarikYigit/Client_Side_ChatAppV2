using System.IO;

namespace Client_Side_ChatApp.Messages
{   
    enum PasswordStrengthOrUsername
    {
        PASSWORD = 0x01,
        USERNAME = 0x02
    }
    public class UserRegisterResponse
    {
        public bool IsAccepted { get; private set; }

        public bool IsPasswordWeak { get; private set; } 
        public byte AssignedUserId { get; private set; }

        public UserRegisterResponse(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return;

            using (MemoryStream ms = new MemoryStream(payload))

            using (BinaryReader reader = new BinaryReader(ms))
            {
                byte status = reader.ReadByte();

                IsAccepted = (status == 0x01);

                if (IsAccepted)
                {

                    AssignedUserId = reader.ReadByte();

                }
                else
                {

                    byte failReason = reader.ReadByte();

                    IsPasswordWeak = (failReason == 0x01);

                }
            }
        }
    }
}
