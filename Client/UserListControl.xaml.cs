using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using EncryptItVC.Client.Models;

namespace EncryptItVC.Client
{
    public partial class UserListControl : UserControl
    {
        private DispatcherTimer _refreshTimer;
        private VoiceManager? _voiceManager;
        private List<User> _users = new();
        
        public event Action<string>? UserRightClicked;
        
        public UserListControl()
        {
            InitializeComponent();
            InitializeRefreshTimer();
        }
        
        public void SetVoiceManager(VoiceManager voiceManager)
        {
            _voiceManager = voiceManager;
        }
        
        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(1); // Refresh every second
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }
        
        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            // Request user list update
            UserListUpdated?.Invoke();
        }
        
        public event Action? UserListUpdated;
        
        public void UpdateUsers(List<User> users)
        {
            _users = users ?? new List<User>();
            RefreshUserList();
        }
        
        public void UpdateUserVoiceStatus(string username, bool isMuted, bool isDeafened)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                user.IsMuted = isMuted;
                user.IsDeafened = isDeafened;
                RefreshUserList(); // Refresh the UI to show updated status
            }
        }
        
        private void RefreshUserList()
        {
            UserListPanel.Children.Clear();
            
            // Group users by channel
            var usersByChannel = _users.GroupBy(u => u.Channel).OrderBy(g => g.Key);
            
            foreach (var channelGroup in usersByChannel)
            {
                // Channel header
                var channelHeader = new TextBlock
                {
                    Text = $"📁 {channelGroup.Key}",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.DarkBlue),
                    Margin = new Thickness(0, 5, 0, 2)
                };
                UserListPanel.Children.Add(channelHeader);
                
                // Users in channel
                foreach (var user in channelGroup.OrderBy(u => u.Username))
                {
                    var userPanel = CreateUserPanel(user);
                    UserListPanel.Children.Add(userPanel);
                }
            }
        }
        
        private StackPanel CreateUserPanel(User user)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(15, 1, 0, 1),
                Tag = user.Username
            };
            
            // Status icons
            var statusText = "";
            var statusColor = Colors.Green;
            
            if (user.IsDeafened)
            {
                statusText = "🔇"; // Deafened
                statusColor = Colors.Red;
            }
            else if (user.IsMuted)
            {
                statusText = "🔇"; // Muted
                statusColor = Colors.Orange;
            }
            else
            {
                statusText = "🔊"; // Speaking
                statusColor = Colors.Green;
            }
            
            var statusIcon = new TextBlock
            {
                Text = statusText,
                Foreground = new SolidColorBrush(statusColor),
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // Username
            var usernameText = new TextBlock
            {
                Text = user.Username,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = user.IsAdmin ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black)
            };
            
            if (user.IsAdmin)
            {
                usernameText.Text = "👑 " + user.Username;
            }
            
            panel.Children.Add(statusIcon);
            panel.Children.Add(usernameText);
            
            // Right-click context menu
            var contextMenu = new ContextMenu();
            
            var volumeMenuItem = new MenuItem { Header = "Volume Control" };
            var volumeSlider = new Slider
            {
                Minimum = 0,
                Maximum = 200,
                Value = (_voiceManager?.GetUserVolume(user.Username) ?? 1.0f) * 100,
                Width = 150,
                Margin = new Thickness(5)
            };
            
            volumeSlider.ValueChanged += (s, e) =>
            {
                _voiceManager?.SetUserVolume(user.Username, (float)(e.NewValue / 100.0));
            };
            
            volumeMenuItem.Header = volumeSlider;
            contextMenu.Items.Add(volumeMenuItem);
            
            var muteMenuItem = new MenuItem 
            { 
                Header = "Mute User", 
                Tag = user.Username 
            };
            muteMenuItem.Click += (s, e) =>
            {
                _voiceManager?.SetUserVolume(user.Username, 0f);
            };
            contextMenu.Items.Add(muteMenuItem);
            
            var resetVolumeMenuItem = new MenuItem 
            { 
                Header = "Reset Volume", 
                Tag = user.Username 
            };
            resetVolumeMenuItem.Click += (s, e) =>
            {
                _voiceManager?.SetUserVolume(user.Username, 1f);
                volumeSlider.Value = 100;
            };
            contextMenu.Items.Add(resetVolumeMenuItem);
            
            panel.ContextMenu = contextMenu;
            
            // Right-click event
            panel.MouseRightButtonUp += (s, e) =>
            {
                UserRightClicked?.Invoke(user.Username);
            };
            
            return panel;
        }
        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
        }
    }
}
