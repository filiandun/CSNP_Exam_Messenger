using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using MessengerLibrary;


namespace MessengerServer
{
    internal class ServerObject
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
}
