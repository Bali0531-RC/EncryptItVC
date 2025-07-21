using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EncryptItVC.Client.Models;

namespace EncryptItVC.Client
{
    public partial class MainWindow : Window
    {
        private static bool _isAnyMainWindowOpen = false;
        private ServerConnection _serverConnection;
        private VoiceManager _voiceManager;
        private ObservableCollection<Channel> _channels;
        private ObservableCollection<ChatMessage> _chatMessages;
        private ObservableCollection<string> _users;
        private ObservableCollection<User> _allUsers;
        private bool _isMicrophoneEnabled = false;
        private bool _isSpeakerEnabled = true;
        private DispatcherTimer _connectionTimer;
        private bool _isReconnecting = false;

        public MainWindow(ServerConnection serverConnection)
        {
            // Prevent multiple MainWindows
            if (_isAnyMainWindowOpen)
            {
                this.Close();
                return;
            }
            _isAnyMainWindowOpen = true;
            
            InitializeComponent();
            _serverConnection = serverConnection;
            _voiceManager = new VoiceManager(_serverConnection);
            
            _channels = new ObservableCollection<Channel>();
            _chatMessages = new ObservableCollection<ChatMessage>();
            _users = new ObservableCollection<string>();
            _allUsers = new ObservableCollection<User>();
            
            ChannelsListBox.ItemsSource = _channels;
            ChatMessagesListBox.ItemsSource = _chatMessages;
            
            // Setup UserListControl
            UserListControl.SetVoiceManager(_voiceManager);
            
            // Setup periodic user list refresh
            var userRefreshTimer = new DispatcherTimer();
            userRefreshTimer.Interval = TimeSpan.FromSeconds(10); // 10 másodpercenként
            userRefreshTimer.Tick += async (s, e) => {
                if (_serverConnection.IsConnected && !_isReconnecting)
                {
                    try
                    {
                        await _serverConnection.GetUsersAsync();
                    }
                    catch { /* Ignore errors */ }
                }
            };
            userRefreshTimer.Start();
            
            _serverConnection.MessageReceived += OnMessageReceived;
            _serverConnection.ConnectionLost += OnConnectionLost;
            
            // Setup voice manager events
            _voiceManager.StatusChanged += OnVoiceStatusChanged;
            
            InitializeUI();
            InitializeConnectionMonitor();
            
            // Load channels after UI is ready
            this.Loaded += async (s, e) => await LoadChannels();
        }
        
        private void OnVoiceStatusChanged(bool isMuted, bool isDeafened)
        {
            Dispatcher.Invoke(() =>
            {
                MuteButton.Content = isMuted ? "🔇" : "🎤";
                MuteButton.Background = isMuted ? 
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red) : 
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                    
                DeafenButton.Content = isDeafened ? "🔇" : "🔊";
                DeafenButton.Background = isDeafened ? 
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red) : 
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            });
        }

        private void InitializeConnectionMonitor()
        {
            _connectionTimer = new DispatcherTimer();
            _connectionTimer.Interval = TimeSpan.FromSeconds(10); // Növeljük az intervallumot
            _connectionTimer.Tick += async (s, e) => await CheckConnection();
            
            // Add automatic refresh timer
            var refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(30); // Lassítjuk 30 másodpercre
            refreshTimer.Tick += async (s, e) => await RefreshData();
            refreshTimer.Start();
            
            // Csak akkor indítsuk el a connection monitoring-ot, ha már tényleg csatlakozva vagyunk
            if (_serverConnection.IsConnected)
            {
                UpdateConnectionStatus(true);
                _connectionTimer.Start();
            }
        }

        private async Task RefreshData()
        {
            if (_serverConnection.IsConnected && !_isReconnecting)
            {
                try
                {
                    await _serverConnection.GetChannelsAsync();
                }
                catch
                {
                    // Ignore refresh errors
                }
            }
        }

        private void InitializeUI()
        {
            UsernameTextBlock.Text = _serverConnection.Username;
            CurrentChannelTextBlock.Text = $"- {_serverConnection.CurrentChannel}";
            
            if (_serverConnection.IsAdmin)
            {
                UserRoleTextBlock.Text = "(Admin)";
                UserRoleTextBlock.Foreground = System.Windows.Media.Brushes.Gold;
                AdminControlsPanel.Visibility = Visibility.Visible;
            }
            else if (_serverConnection.CanCreateChannels)
            {
                UserRoleTextBlock.Text = "(Channel Creator)";
                UserRoleTextBlock.Foreground = System.Windows.Media.Brushes.LightBlue;
            }
            else
            {
                UserRoleTextBlock.Text = "(User)";
            }
            
            CreateChannelButton.IsEnabled = _serverConnection.CanCreateChannels;
            
            // Add system message
            _chatMessages.Add(new ChatMessage
            {
                Username = "SYSTEM",
                Content = $"Welcome to {_serverConnection.CurrentChannel}!",
                Timestamp = DateTime.Now,
                Channel = _serverConnection.CurrentChannel
            });
        }

        private async Task LoadChannels()
        {
            try
            {
                await Task.Delay(500); // Rövid késleltetés a kapcsolat stabilizálásához
                await _serverConnection.GetChannelsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load channels: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task CheckConnection()
        {
            if (_isReconnecting || _serverConnection == null) return;
            
            // Ne ellenőrizzük túl gyakran, ha már tudjuk hogy nincs kapcsolat
            if (!_serverConnection.IsConnected)
            {
                // Csak akkor próbáljunk újracsatlakozni, ha korábban kapcsolódva voltunk
                if (_connectionTimer.IsEnabled) // Ez jelzi, hogy korábban csatlakozva voltunk
                {
                    await HandleConnectionLost();
                }
            }
            else
            {
                UpdateConnectionStatus(true);
            }
        }

        private async Task HandleConnectionLost()
        {
            if (_isReconnecting) return;
            
            _isReconnecting = true;
            _connectionTimer?.Stop(); // Állítsuk le a connection monitoring-ot
            UpdateConnectionStatus(false);
            
            // Add system message about connection loss
            Dispatcher.Invoke(() =>
            {
                _chatMessages.Add(new ChatMessage
                {
                    Username = "SYSTEM",
                    Content = "Connection lost. Attempting to reconnect...",
                    Timestamp = DateTime.Now,
                    Channel = _serverConnection.CurrentChannel ?? "Unknown"
                });
            });
            
            // Try to reconnect (3 attempts)
            bool reconnected = false;
            for (int i = 0; i < 3 && !reconnected; i++)
            {
                await Task.Delay(2000); // Wait 2 seconds between attempts
                reconnected = await AttemptReconnect();
                
                if (!reconnected)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _chatMessages.Add(new ChatMessage
                        {
                            Username = "SYSTEM",
                            Content = $"Reconnection attempt {i + 1} failed...",
                            Timestamp = DateTime.Now,
                            Channel = _serverConnection.CurrentChannel ?? "Unknown"
                        });
                    });
                }
            }
            
            if (reconnected)
            {
                Dispatcher.Invoke(() =>
                {
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = "Reconnected successfully!",
                        Timestamp = DateTime.Now,
                        Channel = _serverConnection.CurrentChannel ?? "Unknown"
                    });
                });
                
                // Reload channels after reconnection
                await LoadChannels();
                UpdateConnectionStatus(true);
                _connectionTimer?.Start(); // Indítsuk újra a monitoring-ot
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = "Failed to reconnect after 3 attempts.",
                        Timestamp = DateTime.Now,
                        Channel = _serverConnection.CurrentChannel ?? "Unknown"
                    });
                });
                
                // Ne mutassunk automatikusan dialógust, csak logoljuk
                // A felhasználó manuálisan tud újracsatlakozni a Disconnect gombbal
            }
            
            _isReconnecting = false;
        }

        private async Task<bool> AttemptReconnect()
        {
            try
            {
                // Try to connect to the server using stored server details
                bool connected = await _serverConnection.ConnectAsync(_serverConnection.ServerHost, _serverConnection.ServerPort);
                
                if (connected && !string.IsNullOrEmpty(_serverConnection.Username))
                {
                    // Try to login with stored credentials
                    bool loginSuccess = await _serverConnection.LoginAsync(_serverConnection.Username, _serverConnection.LastPassword);
                    return loginSuccess;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void OnConnectionLost()
        {
            // Only handle connection loss if we're not already disposed
            if (_serverConnection == null || _isReconnecting) return;
            
            Dispatcher.Invoke(() =>
            {
                _ = HandleConnectionLost();
            });
        }

        private void UpdateConnectionStatus(bool connected)
        {
            Dispatcher.Invoke(() =>
            {
                if (connected)
                {
                    ConnectionStatusTextBlock.Text = "Connected";
                    ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    ConnectionStatusTextBlock.Text = "Not connected";
                    ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                }
            });
        }

        private async Task AttemptReconnection()
        {
            try
            {
                // Use the stored credentials from the server connection
                var serverHost = _serverConnection.ServerHost;
                var serverPort = _serverConnection.ServerPort;
                var username = _serverConnection.Username;
                var password = _serverConnection.LastPassword;
                
                // Create new connection with same credentials
                var newConnection = new ServerConnection();
                var connected = await newConnection.ConnectAsync(serverHost, serverPort);
                
                if (connected)
                {
                    // Try to login with stored credentials
                    var success = await newConnection.LoginAsync(username, password);
                    
                    if (success)
                    {
                    Dispatcher.Invoke(() =>
                    {
                        // Update the connection reference
                        _serverConnection.ConnectionLost -= OnConnectionLost;
                        _serverConnection = newConnection;
                        _serverConnection.MessageReceived += OnMessageReceived;
                        _serverConnection.ConnectionLost += OnConnectionLost;
                        
                        // Update UI to reflect reconnection
                        UpdateConnectionStatus(true);
                        InitializeUI();
                        
                        // Restart connection monitoring
                        InitializeConnectionMonitor();
                        
                        // Load channels
                        _ = LoadChannels();
                        
                        _isReconnecting = false;
                        
                        // Add system message about reconnection
                        _chatMessages.Add(new ChatMessage
                        {
                            Username = "SYSTEM",
                            Content = "Reconnected successfully!",
                            Timestamp = DateTime.Now,
                            Channel = _serverConnection.CurrentChannel
                        });
                    });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _isReconnecting = false;
                            MessageBox.Show("Failed to reconnect. Please try again manually.", "Reconnection Failed", 
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                            
                            // Show login window as fallback
                            var loginWindow = new LoginWindow();
                            loginWindow.Show();
                            Application.Current.MainWindow = loginWindow;
                            this.Close();
                        });
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        _isReconnecting = false;
                        MessageBox.Show("Failed to connect to server. Please try again manually.", "Connection Failed", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        // Show login window as fallback
                        var loginWindow = new LoginWindow();
                        loginWindow.Show();
                        Application.Current.MainWindow = loginWindow;
                        this.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    _isReconnecting = false;
                    MessageBox.Show($"Reconnection error: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Show login window as fallback
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Application.Current.MainWindow = loginWindow;
                    this.Close();
                });
            }
        }

        private void OnMessageReceived(Message message)
        {
            switch (message.Type)
            {
                case "CHANNELS_LIST":
                    LoadChannelsFromMessage(message);
                    break;
                    
                case "CHANNEL_CREATED":
                    var newChannel = new Channel
                    {
                        Name = message.Data["channelName"].ToString(),
                        Owner = message.Data["owner"].ToString(),
                        IsPrivate = (bool)message.Data["isPrivate"],
                        MemberCount = 0
                    };
                    _channels.Add(newChannel);
                    
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = $"Channel '{newChannel.Name}' was created by {newChannel.Owner}",
                        Timestamp = DateTime.Now,
                        Channel = _serverConnection.CurrentChannel
                    });
                    break;
                    
                case "CHANNEL_JOINED":
                    var channelName = message.Data["channelName"].ToString();
                    var members = message.Data["members"] as Newtonsoft.Json.Linq.JArray;
                    
                    _serverConnection.CurrentChannel = channelName;
                    CurrentChannelTextBlock.Text = $"- {channelName}";
                    
                    _users.Clear();
                    foreach (var member in members)
                    {
                        _users.Add(member.ToString());
                    }
                    
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = $"You joined channel '{channelName}'",
                        Timestamp = DateTime.Now,
                        Channel = channelName
                    });
                    break;
                    
                case "CHAT_MESSAGE":
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = message.From,
                        Content = message.Content,
                        Timestamp = message.Timestamp,
                        Channel = message.Channel
                    });
                    
                    // Auto-scroll to bottom
                    if (ChatMessagesListBox.Items.Count > 0)
                    {
                        ChatMessagesListBox.ScrollIntoView(ChatMessagesListBox.Items[ChatMessagesListBox.Items.Count - 1]);
                    }
                    break;
                    
                case "USER_JOINED":
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = message.Content,
                        Timestamp = DateTime.Now,
                        Channel = message.Channel
                    });
                    break;
                    
                case "USER_LEFT":
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = message.Content,
                        Timestamp = DateTime.Now,
                        Channel = message.Channel
                    });
                    break;
                    
                case "USERS_LIST":
                    var channelUsers = message.Data["users"] as Newtonsoft.Json.Linq.JArray;
                    _users.Clear();
                    var usersList = new List<User>();
                    if (channelUsers != null)
                    {
                        foreach (var userObj in channelUsers)
                        {
                            var username = userObj["username"]?.ToString() ?? "";
                            var isMuted = userObj["isMuted"]?.ToObject<bool>() ?? false;
                            var isDeafened = userObj["isDeafened"]?.ToObject<bool>() ?? false;
                            var isAdmin = userObj["isAdmin"]?.ToObject<bool>() ?? false;
                            
                            _users.Add(username);
                            usersList.Add(new User 
                            { 
                                Username = username,
                                IsMuted = isMuted,
                                IsDeafened = isDeafened,
                                IsAdmin = isAdmin
                            });
                        }
                    }
                    
                    // Update UserListControl with fresh data
                    UserListControl.UpdateUsers(usersList);
                    break;
                    
                case "USER_VOICE_STATUS":
                    var statusUsername = message.Data["username"]?.ToString() ?? "";
                    var statusMuted = (bool)(message.Data["isMuted"] ?? false);
                    var statusDeafened = (bool)(message.Data["isDeafened"] ?? false);
                    
                    // Update the user's voice status in the UI
                    UserListControl.UpdateUserVoiceStatus(statusUsername, statusMuted, statusDeafened);
                    break;
                    
                case "PERMISSION_GRANTED":
                    var targetUser = message.Data["username"].ToString();
                    var permission = message.Data["permission"].ToString();
                    
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = $"Permission '{permission}' granted to {targetUser}",
                        Timestamp = DateTime.Now,
                        Channel = _serverConnection.CurrentChannel
                    });
                    break;
                    
                case "ERROR":
                    MessageBox.Show(message.Content, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private void LoadChannelsFromMessage(Message message)
        {
            _channels.Clear();
            var channelsList = message.Data["channels"] as Newtonsoft.Json.Linq.JArray;
            
            foreach (var channelData in channelsList)
            {
                var channel = new Channel
                {
                    Name = channelData["name"].ToString(),
                    Owner = channelData["owner"].ToString(),
                    MemberCount = (int)channelData["memberCount"],
                    IsPrivate = (bool)channelData["isPrivate"],
                    HasPassword = (bool)channelData["hasPassword"]
                };
                _channels.Add(channel);
            }
        }

        private async void ChannelsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChannelsListBox.SelectedItem is Channel selectedChannel)
            {
                if (selectedChannel.Name != _serverConnection.CurrentChannel)
                {
                    string password = "";
                    
                    if (selectedChannel.HasPassword)
                    {
                        var passwordDialog = new PasswordDialog();
                        if (passwordDialog.ShowDialog() == true)
                        {
                            password = passwordDialog.Password;
                        }
                        else
                        {
                            return;
                        }
                    }
                    
                    await _serverConnection.JoinChannelAsync(selectedChannel.Name, password);
                }
            }
        }

        private async void CreateChannelButton_Click(object sender, RoutedEventArgs e)
        {
            var createChannelDialog = new CreateChannelDialog();
            if (createChannelDialog.ShowDialog() == true)
            {
                await _serverConnection.CreateChannelAsync(
                    createChannelDialog.ChannelName,
                    createChannelDialog.IsPrivate,
                    createChannelDialog.Password);
            }
        }

        private async void RefreshChannelsButton_Click(object sender, RoutedEventArgs e)
        {
            await _serverConnection.GetChannelsAsync();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void ChatInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(ChatInputTextBox.Text))
            {
                var messageContent = ChatInputTextBox.Text;
                
                // Add message to local chat immediately for better UX
                _chatMessages.Add(new ChatMessage
                {
                    Username = _serverConnection.Username,
                    Content = messageContent,
                    Timestamp = DateTime.Now,
                    Channel = _serverConnection.CurrentChannel
                });
                
                // Auto-scroll to bottom
                if (ChatMessagesListBox.Items.Count > 0)
                {
                    ChatMessagesListBox.ScrollIntoView(ChatMessagesListBox.Items[ChatMessagesListBox.Items.Count - 1]);
                }
                
                // Send to server
                await _serverConnection.SendChatMessageAsync(messageContent);
                ChatInputTextBox.Clear();
            }
        }

        private void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            _isMicrophoneEnabled = !_isMicrophoneEnabled;
            
            if (_isMicrophoneEnabled)
            {
                try
                {
                    _voiceManager.StartRecording(_serverConnection._udpClient);
                    MicrophoneButton.Content = "🎤 LIVE";
                    MicrophoneButton.Background = System.Windows.Media.Brushes.Red;
                    VoiceStatusTextBlock.Text = "🎤 Transmitting";
                    VoiceStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    VoiceLevelProgressBar.Visibility = Visibility.Visible;
                    
                    // Start voice level monitoring
                    var levelTimer = new DispatcherTimer();
                    levelTimer.Interval = TimeSpan.FromMilliseconds(100);
                    levelTimer.Tick += (s, args) =>
                    {
                        if (_isMicrophoneEnabled)
                        {
                            // Simulate voice level - in real implementation, get from NAudio
                            var level = new Random().Next(0, 100);
                            VoiceLevelProgressBar.Value = level;
                        }
                        else
                        {
                            levelTimer.Stop();
                            VoiceLevelProgressBar.Value = 0;
                            VoiceLevelProgressBar.Visibility = Visibility.Collapsed;
                        }
                    };
                    levelTimer.Start();
                    
                    // Add system message
                    _chatMessages.Add(new ChatMessage
                    {
                        Username = "SYSTEM",
                        Content = "🎤 Microphone activated - You are now transmitting audio",
                        Timestamp = DateTime.Now,
                        Channel = _serverConnection.CurrentChannel
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start microphone: {ex.Message}", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _isMicrophoneEnabled = false;
                }
            }
            else
            {
                _voiceManager.StopRecording();
                MicrophoneButton.Content = "🎤 Microphone";
                MicrophoneButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x79, 0x55, 0x48));
                VoiceStatusTextBlock.Text = "🎤 Ready";
                VoiceStatusTextBlock.Foreground = System.Windows.Media.Brushes.Gray;
                VoiceLevelProgressBar.Visibility = Visibility.Collapsed;
                
                // Add system message
                _chatMessages.Add(new ChatMessage
                {
                    Username = "SYSTEM",
                    Content = "🎤 Microphone deactivated",
                    Timestamp = DateTime.Now,
                    Channel = _serverConnection.CurrentChannel
                });
            }
        }

        private void SpeakerButton_Click(object sender, RoutedEventArgs e)
        {
            _isSpeakerEnabled = !_isSpeakerEnabled;
            
            if (_isSpeakerEnabled)
            {
                SpeakerButton.Content = "🔊 Speaker";
                SpeakerButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x79, 0x55, 0x48));
            }
            else
            {
                SpeakerButton.Content = "🔇 Muted";
                SpeakerButton.Background = System.Windows.Media.Brushes.Gray;
            }
        }
        
        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            _voiceManager.ToggleMute();
        }
        
        private void DeafenButton_Click(object sender, RoutedEventArgs e)
        {
            _voiceManager.ToggleDeafen();
        }

        private async void GrantPermissionButton_Click(object sender, RoutedEventArgs e)
        {
            var permissionDialog = new PermissionDialog();
            if (permissionDialog.ShowDialog() == true)
            {
                await _serverConnection.GrantPermissionAsync(permissionDialog.Username, permissionDialog.Permission);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _connectionTimer?.Stop();
            _serverConnection.Disconnect();
            
            var loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            this.Close();
        }

        private void AudioSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var audioDialog = new AudioSettingsDialog(_voiceManager);
                audioDialog.Owner = this;
                
                if (audioDialog.ShowDialog() == true)
                {
                    // Apply the selected audio settings
                    if (_voiceManager != null)
                    {
                        _voiceManager.SetInputDevice(audioDialog.SelectedInputDevice);
                        _voiceManager.SetOutputDevice(audioDialog.SelectedOutputDevice);
                        _voiceManager.SetVolume(audioDialog.VolumeLevel);
                    }
                    
                    // Show confirmation message
                    MessageBox.Show("Audio settings updated successfully!", "Settings Applied", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening audio settings: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _connectionTimer?.Stop();
                _voiceManager?.Dispose();
                
                // Unsubscribe from events before disconnecting
                if (_serverConnection != null)
                {
                    _serverConnection.MessageReceived -= OnMessageReceived;
                    _serverConnection.ConnectionLost -= OnConnectionLost;
                    _serverConnection.Disconnect();
                }
                
                // Make sure we don't trigger reconnection on manual close
                _serverConnection = null!;
                _isReconnecting = false;
                
                // Reset the static flag so a new MainWindow can be created
                _isAnyMainWindowOpen = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
            
            base.OnClosed(e);
        }
    }
}
