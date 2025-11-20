using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EncryptItVC.ClientLinux.Models
{
    public class ServerConnection
    {
        private TcpClient? _tcpClient;
        public UdpClient? _udpClient;
        private NetworkStream? _stream;
        private bool _isConnected;
        private string _serverHost = "";
        private int _serverPort;
        private string _username = "";
        private bool _isAuthenticated;
        private bool _isAdmin;
        private bool _canCreateChannels;
        private string _currentChannel = "";
        private string _lastPassword = "";

        public event Action<Message>? MessageReceived;
        public event Action? ConnectionLost;

        public string ServerHost
        {
            get => _serverHost;
            set => _serverHost = value;
        }

        public int ServerPort
        {
            get => _serverPort;
            set => _serverPort = value;
        }

        public string LastPassword
        {
            get => _lastPassword;
            set => _lastPassword = value;
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => _isAuthenticated = value;
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => _isAdmin = value;
        }

        public bool CanCreateChannels
        {
            get => _canCreateChannels;
            set => _canCreateChannels = value;
        }

        public string CurrentChannel
        {
            get => _currentChannel;
            set => _currentChannel = value;
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                _serverHost = host;
                _serverPort = port;
                
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _stream = _tcpClient.GetStream();
                
                _udpClient = new UdpClient(0);
                _udpClient.Connect(host, port + 1);
                
                IsConnected = true;
                
                _ = Task.Run(ReceiveMessagesAsync);
                
                Console.WriteLine($"Connected to server {host}:{port}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var message = new Message
            {
                Type = "LOGIN",
                Data = new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["password"] = password
                }
            };
            
            _lastPassword = password;
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            var message = new Message
            {
                Type = "REGISTER",
                Data = new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["password"] = password
                }
            };
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> CreateChannelAsync(string channelName, bool isPrivate, string password = "")
        {
            var message = new Message
            {
                Type = "CREATE_CHANNEL",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = channelName,
                    ["isPrivate"] = isPrivate,
                    ["password"] = password
                }
            };
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> JoinChannelAsync(string channelName, string password = "")
        {
            var message = new Message
            {
                Type = "JOIN_CHANNEL",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = channelName,
                    ["password"] = password
                }
            };
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> SendChatMessageAsync(string content)
        {
            var message = new Message
            {
                Type = "CHAT_MESSAGE",
                Content = content,
                From = _username,
                Channel = _currentChannel
            };
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> GetChannelsAsync()
        {
            var message = new Message
            {
                Type = "GET_CHANNELS"
            };
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> GetUsersAsync()
        {
            var message = new Message
            {
                Type = "GET_USERS",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = CurrentChannel ?? "Lobby"
                }
            };
            
            return await SendMessageAsync(message);
        }

        public async Task<bool> UpdateVoiceStatusAsync(bool isMuted, bool isDeafened)
        {
            var message = new Message
            {
                Type = "UPDATE_VOICE_STATUS",
                Data = new Dictionary<string, object>
                {
                    ["isMuted"] = isMuted,
                    ["isDeafened"] = isDeafened
                }
            };
            
            return await SendMessageAsync(message);
        }

        private async Task<bool> SendMessageAsync(Message message)
        {
            try
            {
                if (_stream == null || !_isConnected || _tcpClient?.Connected != true)
                {
                    return false;
                }
                
                var json = JsonConvert.SerializeObject(message);
                var data = Encoding.UTF8.GetBytes(json);
                
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send message failed: {ex.Message}");
                
                if (_tcpClient?.Connected != true)
                {
                    _isConnected = false;
                    IsAuthenticated = false;
                }
                
                return false;
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];
            
            try
            {
                while (_tcpClient?.Connected == true && _isConnected)
                {
                    if (_stream == null) break;
                    
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) 
                    {
                        Console.WriteLine("Server closed connection");
                        break;
                    }
                    
                    var messageData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    try
                    {
                        var message = JsonConvert.DeserializeObject<Message>(messageData);
                        if (message != null)
                        {
                            MessageReceived?.Invoke(message);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON parsing error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    Console.WriteLine($"Connection lost: {ex.Message}");
                    ConnectionLost?.Invoke();
                }
            }
            finally
            {
                if (_isConnected)
                {
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                _tcpClient?.Close();
                _udpClient?.Close();
                IsConnected = false;
                IsAuthenticated = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnect failed: {ex.Message}");
            }
        }
    }

    public class Message
    {
        public string Type { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string Content { get; set; } = "";
        public string Channel { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    public class Channel
    {
        public string Name { get; set; } = "";
        public string Owner { get; set; } = "";
        public int MemberCount { get; set; }
        public bool IsPrivate { get; set; }
        public bool HasPassword { get; set; }
    }

    public class ChatMessage
    {
        public string Username { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Channel { get; set; } = "";
        
        public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Username}: {Content}";
    }
    
    public class User
    {
        public string Username { get; set; } = "";
        public string Channel { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsMuted { get; set; }
        public bool IsDeafened { get; set; }
        public bool IsOnline { get; set; } = true;
        
        public string DisplayName => IsAdmin ? $"👑 {Username}" : Username;
        public string StatusIcon => IsDeafened ? "🔇" : (IsMuted ? "🔇" : "🔊");
    }
}
