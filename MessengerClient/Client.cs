using System.IO;
using System.Text;
using System.Windows;
using System.Text.Json;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows.Controls;

using MessengerLibrary;
using System.Reflection.PortableExecutable;

namespace MessengerClient
{
    public class Client
    {
        string host = "192.168.0.20";
        int port = 2222;

        private TcpClient tcpClient;

        private StreamWriter streamWriter;
        private StreamReader streamReader;

        private ListBox messageListBox;

        public Client(ListBox messageListBox)
        {
            this.tcpClient = new TcpClient();

            this.messageListBox = messageListBox;
        }

        public void Connect(string messageJson)
        {
            try
            {
                this.tcpClient.Connect(this.host, this.port); // подключение клиента

                this.streamReader = new StreamReader(this.tcpClient.GetStream(), Encoding.UTF8);
                this.streamWriter = new StreamWriter(this.tcpClient.GetStream(), Encoding.UTF8);

                Task.Run(() => ReceiveMessageAsync());

                this.SendMessageAsync(messageJson).Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            await this.streamWriter.WriteLineAsync(message);
            await this.streamWriter.FlushAsync(); // принудительная отправка
        }

        public async Task<string> ReceiveMessageAsync()
        {
            while (true)
            {
                try
                {
                    // считываем ответ
                    string? requestJson = await this.streamReader.ReadLineAsync();

                    if (requestJson == null)
                    {
                        continue;
                    }

                    // дисеарилизуем
                    Request request = JsonSerializer.Deserialize<Request>(requestJson);

                    switch (request.messageType)
                    {
                        case MessageType.Message:
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.messageListBox.Items.Add(new MessageListBoxItem(request.userName, request.userMessage, request.messageType, request.userColor));
                                this.messageListBox.Items.Refresh();
                            });
                            
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}

