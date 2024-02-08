using System.Net;
using System.Text.Json;
using System.Net.Sockets;
using MessengerLibrary;

namespace MessengerServer
{
    class ServerObject
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, 2222); // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения

        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = this.clients.FirstOrDefault(c => c.Id == id);

            // и удаляем его из списка подключений
            if (client != null)
            {
                clients.Remove(client);
            }

            client?.Close();
        }

        // прослушивание входящих подключений
        protected internal async Task ListenAsync()
        {
            try
            {
                this.tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                    ClientObject clientObject = new ClientObject(tcpClient, this);

                    this.clients.Add(clientObject);

                    Task.Run(clientObject.ProcessAsync);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                this.Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal async Task BroadcastMessageAsync(Request request, string id)
        {
            if (request.userMessage == "Вошёл в чат")
            {
                request.userMessage = $"{request.userName} вошёл в чат";
                request.messageType = MessageType.Server;
                string requestForAll = JsonSerializer.Serialize(request);

                foreach (var client in clients)
                {
                    await client.streamWriter.WriteLineAsync(requestForAll); // передача данных
                    await client.streamWriter.FlushAsync();
                }
            }
            else
            {
                request.messageType = MessageType.Incoming;
                string requestForAll = JsonSerializer.Serialize(request);

                Request requestForSender = request;
                requestForSender.messageType = MessageType.Outgoing;
                string requestForSenderJson = JsonSerializer.Serialize(requestForSender);

                foreach (var client in clients)
                {
                    if (client.Id != id) // если id клиента не равно id отправителя
                    {
                        await client.streamWriter.WriteLineAsync(requestForAll); // передача данных
                        await client.streamWriter.FlushAsync();
                    }
                    else
                    {
                        await client.streamWriter.WriteLineAsync(requestForSenderJson); // передача данных
                        await client.streamWriter.FlushAsync();
                    }
                }
            }
        }

        // отключение всех клиентов
        protected internal void Disconnect()
        {
            Console.WriteLine("Сервер останавливается. Отключение...");

            foreach (ClientObject client in this.clients)
            {
                this.RemoveConnection(client.Id);
                // client.Close(); // было так
            }

            this.tcpListener.Stop(); //остановка сервера
        }
    }

    class ClientObject
    {
        protected internal string Id { get; } = Guid.NewGuid().ToString();

        protected internal StreamReader streamReader { get; }
        protected internal StreamWriter streamWriter { get; }

        TcpClient client;
        ServerObject server; // объект сервера

        private string clientName;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            this.client = tcpClient;
            this.server = serverObject;

            // получаем NetworkStream для взаимодействия с сервером
            this.streamReader = new StreamReader(this.client.GetStream());
            this.streamWriter = new StreamWriter(this.client.GetStream());
        }

        public async Task ProcessAsync()
        {
            try
            {
                // получаем имя пользователя
                string firstMessageJson = await this.streamReader.ReadLineAsync();
                Request firstRequest = JsonSerializer.Deserialize<Request>(firstMessageJson);
                this.clientName = firstRequest.userName;

                // посылаем сообщение о входе в чат всем подключенным пользователям
                await server.BroadcastMessageAsync(firstRequest, Id);
                Console.WriteLine($"{this.clientName} вошел в чат");

                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        string messageJson = await this.streamReader.ReadLineAsync();

                        if (messageJson == null)
                        {
                            continue;
                        }

                        // Console.WriteLine(messageJson);
                        Request request = JsonSerializer.Deserialize<Request>(messageJson);

                        await server.BroadcastMessageAsync(request, this.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{this.clientName} покинул чат");
                        Console.WriteLine(ex.Message);

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                this.server.RemoveConnection(Id);
            }
        }

        // закрытие подключения
        protected internal void Close()
        {
            this.streamReader.Close();
            this.streamWriter.Close();
            this.client.Close();
        }
    }


    internal class Program
    {
        private static async Task StartServer()
        {
            ServerObject server = new ServerObject(); // создаем сервер
            await server.ListenAsync(); // запускаем сервер
        }

        static void Main(string[] args)
        {
            StartServer().Wait();
        }
    }
}
