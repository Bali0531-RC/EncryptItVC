using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EncryptItVC.ClientLinux.Models;
using System;
using System.Threading.Tasks;

namespace EncryptItVC.ClientLinux.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly ServerConnection _serverConnection;
        
        [ObservableProperty]
        private string _serverHost = "127.0.0.1";
        
        [ObservableProperty]
        private string _serverPort = "7777";
        
        [ObservableProperty]
        private string _username = "";
        
        [ObservableProperty]
        private string _password = "";
        
        [ObservableProperty]
        private string _statusMessage = "";
        
        [ObservableProperty]
        private bool _isConnecting = false;
        
        public event Action<ServerConnection>? LoginSuccessful;
        
        public LoginViewModel()
        {
            _serverConnection = new ServerConnection();
        }
        
        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter username and password";
                return;
            }
            
            if (string.IsNullOrWhiteSpace(ServerHost) || string.IsNullOrWhiteSpace(ServerPort))
            {
                StatusMessage = "Please enter server host and port";
                return;
            }
            
            if (!int.TryParse(ServerPort, out int port))
            {
                StatusMessage = "Invalid port number";
                return;
            }
            
            IsConnecting = true;
            StatusMessage = "Connecting to server...";
            
            try
            {
                var connected = await _serverConnection.ConnectAsync(ServerHost, port);
                
                if (!connected)
                {
                    StatusMessage = "Failed to connect to server";
                    IsConnecting = false;
                    return;
                }
                
                StatusMessage = "Logging in...";
                var success = await _serverConnection.LoginAsync(Username, Password);
                
                if (success)
                {
                    // Wait for login response from server
                    for (int i = 0; i < 20; i++) // Wait up to 2 seconds
                    {
                        await Task.Delay(100);
                        
                        if (_serverConnection.IsAuthenticated)
                        {
                            StatusMessage = "Login successful!";
                            await Task.Delay(300); // Brief pause to show success message
                            LoginSuccessful?.Invoke(_serverConnection);
                            return;
                        }
                    }
                    
                    StatusMessage = "Login failed - No response from server";
                }
                else
                {
                    StatusMessage = "Login failed - Could not send request";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsConnecting = false;
            }
        }
        
        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter username and password";
                return;
            }
            
            if (string.IsNullOrWhiteSpace(ServerHost) || string.IsNullOrWhiteSpace(ServerPort))
            {
                StatusMessage = "Please enter server host and port";
                return;
            }
            
            if (!int.TryParse(ServerPort, out int port))
            {
                StatusMessage = "Invalid port number";
                return;
            }
            
            IsConnecting = true;
            StatusMessage = "Connecting to server...";
            
            try
            {
                var connected = await _serverConnection.ConnectAsync(ServerHost, port);
                
                if (!connected)
                {
                    StatusMessage = "Failed to connect to server";
                    IsConnecting = false;
                    return;
                }
                
                StatusMessage = "Registering...";
                var success = await _serverConnection.RegisterAsync(Username, Password);
                
                await Task.Delay(500);
                StatusMessage = success ? "Registration successful! Please login." : "Registration failed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsConnecting = false;
            }
        }
    }
}
