using System.Collections.ObjectModel;
using System.Windows.Input;
using EncryptItVC.MobileClient.Models;
using EncryptItVC.MobileClient.Services;
using Newtonsoft.Json.Linq;

namespace EncryptItVC.MobileClient.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly ServerConnection _serverConnection;
    private readonly VoiceManager _voiceManager;
    private string _serverName = "Server";
    private string _connectionStatus = "Connected";
    private string _newMessage = "";
    private bool _isConnected = true;
    private bool _isMuted = false;
    private bool _isDeafened = false;
    private Channel? _selectedChannel;

    public MainViewModel(ServerConnection serverConnection, VoiceManager voiceManager)
    {
        _serverConnection = serverConnection;
        _voiceManager = voiceManager;
        
        Channels = new ObservableCollection<Channel>();
        UsersInChannel = new ObservableCollection<User>();
        ChatMessages = new ObservableCollection<ChatMessage>();

        InitializeCommands();
        InitializeEvents();
    }

    public ObservableCollection<Channel> Channels { get; }
    public ObservableCollection<User> UsersInChannel { get; }
    public ObservableCollection<ChatMessage> ChatMessages { get; }

    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public string NewMessage
    {
        get => _newMessage;
        set => SetProperty(ref _newMessage, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set => SetProperty(ref _isMuted, value);
    }

    public bool IsDeafened
    {
        get => _isDeafened;
        set => SetProperty(ref _isDeafened, value);
    }

    public Channel? SelectedChannel
    {
        get => _selectedChannel;
        set => SetProperty(ref _selectedChannel, value);
    }

    public ICommand JoinChannelCommand { get; private set; } = null!;
    public ICommand CreateChannelCommand { get; private set; } = null!;
    public ICommand SendMessageCommand { get; private set; } = null!;
    public ICommand ToggleMuteCommand { get; private set; } = null!;
    public ICommand ToggleDeafenCommand { get; private set; } = null!;
    public ICommand PushToTalkCommand { get; private set; } = null!;
    public ICommand VoiceSettingsCommand { get; private set; } = null!;

    private void InitializeCommands()
    {
        JoinChannelCommand = new RelayCommand<Channel>(JoinChannel);
        CreateChannelCommand = new RelayCommand(CreateChannel);
        SendMessageCommand = new RelayCommand(SendMessage, () => !string.IsNullOrEmpty(NewMessage));
        ToggleMuteCommand = new RelayCommand(ToggleMute);
        ToggleDeafenCommand = new RelayCommand(ToggleDeafen);
        PushToTalkCommand = new RelayCommand(PushToTalk);
        VoiceSettingsCommand = new RelayCommand(OpenVoiceSettings);
    }

    private void InitializeEvents()
    {
        _serverConnection.MessageReceived += OnMessageReceived;
        _serverConnection.ConnectionStatusChanged += OnConnectionStatusChanged;
        _voiceManager.MuteStatusChanged += OnMuteStatusChanged;
        _voiceManager.DeafenStatusChanged += OnDeafenStatusChanged;
    }

    private async void JoinChannel(Channel? channel)
    {
        if (channel == null) return;

        try
        {
            await _serverConnection.JoinChannelAsync(channel.Name);
            SelectedChannel = channel;
            
            // Update users in channel
            UsersInChannel.Clear();
            foreach (var user in channel.Users)
            {
                UsersInChannel.Add(user);
            }
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Join channel error: {ex.Message}");
        }
    }

    private async void CreateChannel()
    {
        // This would typically open a dialog to get channel details
        var channelName = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Create Channel", 
            "Enter channel name:");

        if (!string.IsNullOrEmpty(channelName))
        {
            // Send create channel request to server
            // Implementation depends on server protocol
        }
    }

    private async void SendMessage()
    {
        if (string.IsNullOrEmpty(NewMessage)) return;

        try
        {
            await _serverConnection.SendChatMessageAsync(NewMessage);
            NewMessage = "";
            ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Send message error: {ex.Message}");
        }
    }

    private void ToggleMute()
    {
        _voiceManager.IsMuted = !_voiceManager.IsMuted;
    }

    private void ToggleDeafen()
    {
        _voiceManager.IsDeafened = !_voiceManager.IsDeafened;
    }

    private async void PushToTalk()
    {
        if (!IsMuted)
        {
            await _voiceManager.StartRecording();
        }
    }

    private async void OpenVoiceSettings()
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }

    private void OnMessageReceived(object? sender, string message)
    {
        try
        {
            var json = JObject.Parse(message);
            var type = json["type"]?.ToString();

            switch (type)
            {
                case "chat_message":
                    var chatMessage = new ChatMessage
                    {
                        Username = json["from"]?.ToString() ?? "Unknown",
                        Message = json["content"]?.ToString() ?? "",
                        Channel = json["channel"]?.ToString() ?? "",
                        Timestamp = DateTime.Now
                    };
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ChatMessages.Add(chatMessage);
                    });
                    break;

                case "channel_list":
                    var channels = json["channels"]?.ToObject<List<Channel>>();
                    if (channels != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Channels.Clear();
                            foreach (var channel in channels)
                            {
                                Channels.Add(channel);
                            }
                        });
                    }
                    break;

                case "user_list":
                    var users = json["users"]?.ToObject<List<User>>();
                    if (users != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UsersInChannel.Clear();
                            foreach (var user in users)
                            {
                                UsersInChannel.Add(user);
                            }
                        });
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Message processing error: {ex.Message}");
        }
    }

    private void OnConnectionStatusChanged(object? sender, bool isConnected)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsConnected = isConnected;
            ConnectionStatus = isConnected ? "Connected" : "Disconnected";
        });
    }

    private void OnMuteStatusChanged(object? sender, bool isMuted)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsMuted = isMuted;
        });
    }

    private void OnDeafenStatusChanged(object? sender, bool isDeafened)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsDeafened = isDeafened;
        });
    }
}
