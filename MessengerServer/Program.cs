using System.Net;
using System.Text.Json;
using System.Net.Sockets;

using MessengerLibrary;


namespace MessengerServer
{
    internal class Program
    {
        static void Main()
        {
            ServerObject server = new ServerObject(); // создаем сервер
            //Task.Run(() => server.ListenAsync()).Wait();
            server.ListenAsync().Wait();
        }
    }
}
