namespace ClientSideChatApp.Messages
{
    internal class LoginResponse
    {
        public bool IsAccepted { get; private set; }
        public byte AssignedUserId { get; private set; }

        public LoginResponse(byte[] payload)
        {
            if (payload != null && payload.Length > 0)
            {
                IsAccepted = payload[0] == 0x01;

                if (IsAccepted && payload.Length > 1)
                {
                    AssignedUserId = payload[1];
                }
            }
        }
    }
}