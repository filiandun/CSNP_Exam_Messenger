using System.Windows.Media;

namespace MessengerLibrary
{
    public enum MessageType
    {
        SignIn,
        SignUp,
        Message
    }

    public class Request
    {
        public object data { get; set; }

        public MessageType messageType { get; set; }
    }

    public class SignIn
    {
        public string userLogin { get; set; }
        public string userPassword { get; set; }
    }

    public class SignUp
    {
        public string userLogin { get; set; }
        public string userPassword { get; set; }

        public string userName { get; set; }
        public string userColor { get; set; }
    }

    public class Message
    {
        public string userLogin { get; set; }

        public string userMessage { get; set; }
    }

}
