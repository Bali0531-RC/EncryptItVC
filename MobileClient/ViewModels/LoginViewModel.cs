using System.Collections.ObjectModel;
using System.Windows.Input;
using EncryptItVC.MobileClient.Services;

namespace EncryptItVC.MobileClient.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ServerConnection _serverConnection;
    private string _serverAddress = "127.0.0.1:7777";
    private string _username = "";
    private string _password = "";
    private string _statusMessage = "Not connected";
    private bool _isConnected = false;
    private bool _isConnecting = false;

    public LoginViewModel(ServerConnection serverConnection)
    {
        _serverConnection = serverConnection;
        
        ConnectCommand = new RelayCommand(Connect, () => !IsConnecting && !string.IsNullOrEmpty(ServerAddress));
        LoginCommand = new RelayCommand(Login, () => IsConnected && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password));
        RegisterCommand = new RelayCommand(Register, () => IsConnected && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password));
        QuickConnectCommand = new RelayCommand(QuickConnect);
    }

    public string ServerAddress
    {
        get => _serverAddress;
        set
        {
            SetProperty(ref _serverAddress, value);
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value);
            ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            SetProperty(ref _isConnected, value);
            ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            SetProperty(ref _isConnecting, value);
            OnPropertyChanged(nameof(IsNotConnecting));
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsNotConnecting => !IsConnecting;

    public string ConnectButtonText => IsConnecting ? "Connecting..." : IsConnected ? "Disconnect" : "Connect";

    public ICommand ConnectCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand QuickConnectCommand { get; }

    private async void Connect()
    {
        try
        {
            IsConnecting = true;
            StatusMessage = "Connecting...";

            if (IsConnected)
            {
                await _serverConnection.DisconnectAsync();
                IsConnected = false;
                StatusMessage = "Disconnected";
                return;
            }

            var parts = ServerAddress.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 ? int.Parse(parts[1]) : 7777;

            await _serverConnection.ConnectAsync(host, port);
            IsConnected = true;
            StatusMessage = $"Connected to {host}:{port}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection failed: {ex.Message}";
            IsConnected = false;
        }
        finally
        {
            IsConnecting = false;
            OnPropertyChanged(nameof(ConnectButtonText));
        }
    }

    private async void Login()
    {
        try
        {
            StatusMessage = "Logging in...";
            var success = await _serverConnection.LoginAsync(Username, Password);
            
            if (success)
            {
                StatusMessage = "Login successful";
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                StatusMessage = "Login failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Login error: {ex.Message}";
        }
    }

    private async void Register()
    {
        try
        {
            StatusMessage = "Registering...";
            var success = await _serverConnection.RegisterAsync(Username, Password);
            
            if (success)
            {
                StatusMessage = "Registration successful";
            }
            else
            {
                StatusMessage = "Registration failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Registration error: {ex.Message}";
        }
    }

    private void QuickConnect()
    {
        ServerAddress = "127.0.0.1:7777";
        ConnectCommand.Execute(null);
    }
}
