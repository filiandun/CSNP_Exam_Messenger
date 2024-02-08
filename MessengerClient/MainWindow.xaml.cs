using System.Windows;
using System.Text.Json;
using System.Windows.Media;

using MessengerLibrary;


namespace MessengerClient
{
    public partial class MainWindow : Window
    {
        private Client client;

        private string userName;
        private string userColor;


        public MainWindow()
        {
            InitializeComponent();

            AuthorizationWindow authorization = new AuthorizationWindow();
            if (authorization.ShowDialog() == false)
            {
                this.Close();
            }

            this.userName = authorization.userName;
            this.userColor = authorization.userColor;

            this.client = new Client(this.messageListBox);

            Request request = new Request { userName = this.userName, userMessage = "Вошёл в чат", userColor = this.userColor, timestamp = DateTime.Now.ToString("H:mm"), messageType = MessageType.Server };
            string messageJson = JsonSerializer.Serialize(request);

            this.client.Connect(messageJson);
        }


        private async void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.sendMessageButton.IsEnabled = false;

            string message = this.messageTextBox.Text;

            if (!string.IsNullOrWhiteSpace(message))
            {
                Request request = new Request { userName = this.userName, userMessage = message, userColor = this.userColor, timestamp = DateTime.Now.ToString("H:mm"), messageType = MessageType.Incoming };
                string requestJson = JsonSerializer.Serialize(request);

                await this.client.SendMessageAsync(requestJson);

                this.messageTextBox.Text = "";
                this.messageTextBox.Focus();
            }

            this.sendMessageButton.IsEnabled = true;
        }

        private void messageTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.messageTextBox.Text == "Тут введите ваше сообщение")
            {
                this.messageTextBox.Text = "";
            }
        }

        private void messageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.messageTextBox.Text))
            {
                this.messageTextBox.Text = "Тут введите ваше сообщение";
            }
        }
    }
}