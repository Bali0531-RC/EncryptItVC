using System.Windows;

namespace EncryptItVC.Client
{
    public partial class PermissionDialog : Window
    {
        public string Username { get; private set; }
        public string Permission { get; private set; }

        public PermissionDialog()
        {
            InitializeComponent();
        }

        private void GrantButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter a username.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Username = UsernameTextBox.Text.Trim();
            
            if (AdminRadio.IsChecked == true)
            {
                Permission = "ADMIN";
            }
            else if (CreateChannelsRadio.IsChecked == true)
            {
                Permission = "CREATE_CHANNELS";
            }

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
