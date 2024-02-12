using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using MessengerLibrary;


namespace MessengerServer
{
    internal class ClientObject
    {
        protected internal string Id { get; } = Guid.NewGuid().ToString();

        protected internal StreamReader streamReader { get; }
        protected internal StreamWriter streamWriter { get; }

        TcpClient client;
        ServerObject server; // объект сервера

        // ВМЕСТО ЭТИХ СТРОК, ТУТ БУДЕТ ОБЪЕКТ USERS ИЗ БД
        private string clientName;
        private string clientColor;

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

                        Request request = JsonSerializer.Deserialize<Request>(messageJson);

                        switch (request.messageType)
                        {
                            case MessageType.SignIn:

                                SignIn signIn = request.data as SignIn;

                                this.clientName = signIn.userLogin;
                                //this.clientColor = signIn.userColor;

                                break;
                        }

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
}
