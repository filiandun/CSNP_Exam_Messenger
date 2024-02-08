using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;

using MessengerLibrary;

namespace MessengerClient
{
    public partial class AuthorizationWindow : Window
    {
        private SolidColorBrush defaultColorBrush = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
        private SolidColorBrush errorColorBrush = new SolidColorBrush(Colors.Red);

        public string userName;
        public string userColor;


        public AuthorizationWindow()
        {
            InitializeComponent();
        }


        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            string pattern = @"^[A-Za-z]+[0-9]*$";
            Regex regex = new Regex(pattern);

            string name = this.nameTextBox.Text;
            if (!regex.IsMatch(name))
            {
                this.nameTextBox.BorderBrush = this.errorColorBrush;
                this.nameTextBox.Text = "Некорректный никнейм";

                return;
            }

            this.userName = name;
            this.userColor = this.colorCanvas.ColorName;

            this.DialogResult = true;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        private void nickTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.nameTextBox.BorderBrush = this.defaultColorBrush;
            if (this.nameTextBox.Text == "Тут введите ваш никнейм" || this.nameTextBox.Text == "Некорректный никнейм")
            {
                this.nameTextBox.Text = "";
            }
        }

        private void nickTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.nameTextBox.Text))
            {
                this.nameTextBox.Text = "Тут введите ваш никнейм";
            }
        }
    }
}
