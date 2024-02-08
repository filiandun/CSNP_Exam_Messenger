using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using MessengerLibrary;

namespace MessengerClient
{
    public partial class MessageListBoxItem : UserControl
    {
        public MessageListBoxItem(string name, string message, MessageType messageDirection, string userColor)
        {
            InitializeComponent();

            this.messageTextBlock.Text = message;
            this.timeTextBlock.Text = DateTime.Now.ToString("H:mm");

            if (messageDirection == MessageType.Outgoing)
            {
                this.nameTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(userColor));

                this.nameTextBlock.Text = "Вы";
                this.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else if (messageDirection == MessageType.Incoming)
            {
                this.nameTextBlock.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString(userColor));

                this.nameTextBlock.Text = name;
                this.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (messageDirection == MessageType.Server)
            {
                this.nameTextBlock.Foreground = new SolidColorBrush(Colors.Red);

                this.nameTextBlock.Text = "Сервер";
                this.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }
    }
}
