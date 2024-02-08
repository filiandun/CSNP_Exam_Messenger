using System.Windows.Media;

namespace MessengerLibrary
{
    public enum MessageType
    {
        Outgoing,
        Incoming,
        Server
    }

    public class Request
    {
        public string userName { get; set; }
        public string userMessage { get; set; }

        public string userColor { get; set; }

        public string timestamp { get; set; }

        public MessageType messageType { get; set; }
    }
}
