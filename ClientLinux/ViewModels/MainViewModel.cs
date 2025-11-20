using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EncryptItVC.ClientLinux.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EncryptItVC.ClientLinux.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly ServerConnection _serverConnection;
        private readonly VoiceManager _voiceManager;
        
        [ObservableProperty]
        private string _username = "";
        
        [ObservableProperty]
        private string _currentChannel = "";
        
        [ObservableProperty]
        private string _chatInput = "";
        
        [ObservableProperty]
        private bool _isVoiceActive = false;
        
        [ObservableProperty]
        private bool _isMuted = false;
        
        [ObservableProperty]
        private bool _isDeafened = false;
        
        public ObservableCollection<Channel> Channels { get; } = new();
        public ObservableCollection<ChatMessage> ChatMessages { get; } = new();
        public ObservableCollection<User> Users { get; } = new();
        
        public MainViewModel() : this(new ServerConnection())
        {
            // Design-time constructor
        }
        
        public MainViewModel(ServerConnection serverConnection)
        {
            _serverConnection = serverConnection;
            _voiceManager = new VoiceManager(_serverConnection);
            
            Username = _serverConnection.Username;
            CurrentChannel = _serverConnection.CurrentChannel;
            
            _serverConnection.MessageReceived += OnMessageReceived;
            
            _voiceManager.StatusChanged += (muted, deafened) =>
            {
                IsMuted = muted;
                IsDeafened = deafened;
            };
            
            _ = LoadChannels();
            _ = LoadUsers();
        }
        
        private async Task LoadChannels()
        {
            await _serverConnection.GetChannelsAsync();
        }
        
        private async Task LoadUsers()
        {
            await _serverConnection.GetUsersAsync();
        }
        
        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(ChatInput)) return;
            
            var message = new ChatMessage
            {
                Username = Username,
                Content = ChatInput,
                Timestamp = DateTime.Now,
                Channel = CurrentChannel
            };
            
            ChatMessages.Add(message);
            
            await _serverConnection.SendChatMessageAsync(ChatInput);
            ChatInput = "";
        }
        
        [RelayCommand]
        private void ToggleVoice()
        {
            if (IsVoiceActive)
            {
                _voiceManager.StopRecording();
                IsVoiceActive = false;
            }
            else
            {
                _voiceManager.StartRecording(_serverConnection._udpClient);
                IsVoiceActive = true;
            }
        }
        
        [RelayCommand]
        private void ToggleMute()
        {
            _voiceManager.ToggleMute();
        }
        
        [RelayCommand]
        private void ToggleDeafen()
        {
            _voiceManager.ToggleDeafen();
        }
        
        [RelayCommand]
        private async Task JoinChannel(Channel? channel)
        {
            if (channel == null) return;
            
            await _serverConnection.JoinChannelAsync(channel.Name);
        }
        
        [RelayCommand]
        private async Task RefreshChannels()
        {
            await LoadChannels();
        }
        
        [RelayCommand]
        private async Task RefreshUsers()
        {
            await LoadUsers();
        }
        
        private void OnMessageReceived(Message message)
        {
            switch (message.Type)
            {
                case "LOGIN_SUCCESS":
                    Username = message.Data["username"].ToString() ?? "";
                    CurrentChannel = message.Data["currentChannel"].ToString() ?? "";
                    break;
                    
                case "CHANNELS_LIST":
                    Channels.Clear();
                    if (message.Data["channels"] is Newtonsoft.Json.Linq.JArray channelsList)
                    {
                        foreach (var channelData in channelsList)
                        {
                            var channel = new Channel
                            {
                                Name = channelData["name"]?.ToString() ?? "",
                                Owner = channelData["owner"]?.ToString() ?? "",
                                MemberCount = channelData["memberCount"]?.ToObject<int>() ?? 0,
                                IsPrivate = channelData["isPrivate"]?.ToObject<bool>() ?? false,
                                HasPassword = channelData["hasPassword"]?.ToObject<bool>() ?? false
                            };
                            Channels.Add(channel);
                        }
                    }
                    break;
                    
                case "CHANNEL_JOINED":
                    CurrentChannel = message.Data["channelName"].ToString() ?? "";
                    ChatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = $"Joined channel: {CurrentChannel}",
                        Timestamp = DateTime.Now
                    });
                    _ = LoadUsers();
                    break;
                    
                case "CHAT_MESSAGE":
                    if (message.From != Username) // Don't add our own messages again
                    {
                        ChatMessages.Add(new ChatMessage
                        {
                            Username = message.From,
                            Content = message.Content,
                            Timestamp = message.Timestamp,
                            Channel = message.Channel
                        });
                    }
                    break;
                    
                case "USER_JOINED":
                case "USER_LEFT":
                    ChatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = message.Content,
                        Timestamp = DateTime.Now
                    });
                    _ = LoadUsers();
                    break;
                    
                case "USERS_LIST":
                    Users.Clear();
                    if (message.Data["users"] is Newtonsoft.Json.Linq.JArray usersList)
                    {
                        foreach (var userData in usersList)
                        {
                            var user = new User
                            {
                                Username = userData["username"]?.ToString() ?? "",
                                IsMuted = userData["isMuted"]?.ToObject<bool>() ?? false,
                                IsDeafened = userData["isDeafened"]?.ToObject<bool>() ?? false,
                                IsAdmin = userData["isAdmin"]?.ToObject<bool>() ?? false
                            };
                            Users.Add(user);
                        }
                    }
                    break;
                    
                case "USER_VOICE_STATUS":
                    var statusUsername = message.Data["username"]?.ToString() ?? "";
                    var statusUser = Users.FirstOrDefault(u => u.Username == statusUsername);
                    if (statusUser != null)
                    {
                        var isMuted = message.Data["isMuted"];
                        var isDeafened = message.Data["isDeafened"];
                        statusUser.IsMuted = isMuted is bool b1 ? b1 : false;
                        statusUser.IsDeafened = isDeafened is bool b2 ? b2 : false;
                    }
                    break;
            }
        }
    }
}
