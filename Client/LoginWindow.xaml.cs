using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EncryptItVC.Client.Models;

namespace EncryptItVC.Client
{
    public partial class LoginWindow : Window
    {
        private ServerConnection _serverConnection;
        private bool _isMainWindowOpened = false;
        
        public LoginWindow()
        {
            InitializeComponent();
            _serverConnection = new ServerConnection();
            _serverConnection.MessageReceived += OnMessageReceived;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerHostTextBox.Text) || 
                string.IsNullOrWhiteSpace(ServerPortTextBox.Text))
            {
                MessageBox.Show("Please enter server host and port.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (!int.TryParse(ServerPortTextBox.Text, out int port))
            {
                MessageBox.Show("Please enter a valid port number.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            ShowLoading("Connecting to server...");
            
            var success = await _serverConnection.ConnectAsync(ServerHostTextBox.Text, port);
            
            HideLoading();
            
            if (success)
            {
                StatusTextBlock.Text = "Connected to server";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                ConnectButton.IsEnabled = false;
                LoginButton.IsEnabled = true;
                RegisterButton.IsEnabled = true;
                ServerHostTextBox.IsEnabled = false;
                ServerPortTextBox.IsEnabled = false;
            }
            else
            {
                StatusTextBlock.Text = "Connection failed";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || 
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter username and password.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            ShowLoading("Logging in...");
            
            var success = await _serverConnection.LoginAsync(UsernameTextBox.Text, PasswordBox.Password);
            
            HideLoading();
            
            if (!success)
            {
                StatusTextBlock.Text = "Login failed";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || 
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter username and password.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            ShowLoading("Registering...");
            
            var success = await _serverConnection.RegisterAsync(UsernameTextBox.Text, PasswordBox.Password);
            
            HideLoading();
            
            if (!success)
            {
                StatusTextBlock.Text = "Registration failed";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void OnMessageReceived(Message message)
        {
            switch (message.Type)
            {
                case "LOGIN_SUCCESS":
                    // Prevent multiple main windows
                    if (_isMainWindowOpened) return;
                    _isMainWindowOpened = true;
                    
                    _serverConnection.Username = message.Data["username"].ToString();
                    _serverConnection.IsAuthenticated = true;
                    _serverConnection.IsAdmin = (bool)message.Data["isAdmin"];
                    _serverConnection.CanCreateChannels = (bool)message.Data["canCreateChannels"];
                    _serverConnection.CurrentChannel = message.Data["currentChannel"].ToString();
                    
                    StatusTextBlock.Text = "Login successful";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                    
                    // Open main window and set as application main window
                    var mainWindow = new MainWindow(_serverConnection);
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    this.Close();
                    break;
                    
                case "LOGIN_FAILED":
                    StatusTextBlock.Text = message.Content;
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    break;
                    
                case "REGISTER_SUCCESS":
                    StatusTextBlock.Text = message.Content;
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                    MessageBox.Show("Registration successful! You can now log in.", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                    
                case "REGISTER_FAILED":
                    StatusTextBlock.Text = message.Content;
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    break;
            }
        }

        private void ShowLoading(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
            LoadingProgressBar.Visibility = Visibility.Visible;
            ConnectButton.IsEnabled = false;
            LoginButton.IsEnabled = false;
            RegisterButton.IsEnabled = false;
        }

        private void HideLoading()
        {
            LoadingProgressBar.Visibility = Visibility.Collapsed;
            
            if (_serverConnection.IsConnected)
            {
                LoginButton.IsEnabled = true;
                RegisterButton.IsEnabled = true;
            }
            else
            {
                ConnectButton.IsEnabled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _serverConnection?.Disconnect();
            base.OnClosed(e);
        }
    }
}
