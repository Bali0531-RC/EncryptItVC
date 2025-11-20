using System.Windows;

namespace EncryptItVC.Client
{
    public partial class CreateChannelDialog : Window
    {
        public string ChannelName { get; private set; }
        public bool IsPrivate { get; private set; }
        public string Password { get; private set; }

        public CreateChannelDialog()
        {
            InitializeComponent();
        }

        private void IsPrivateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBox.IsEnabled = true;
        }

        private void IsPrivateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.IsEnabled = false;
            PasswordBox.Password = "";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChannelNameTextBox.Text))
            {
                MessageBox.Show("Please enter a channel name.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ChannelName = ChannelNameTextBox.Text.Trim();
            IsPrivate = IsPrivateCheckBox.IsChecked == true;
            Password = PasswordBox.Password;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
