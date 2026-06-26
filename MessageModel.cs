//Main class used for storing message info, will be used when a message send instruction is sent
namespace Client_Side_ChatApp.Models
{
    public class MessageModel
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Reciever { get; set; }
        public string Timestamp { get; set; }
    }
}