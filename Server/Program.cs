using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EncryptItVC.Server
{
    public class ServerConfig
    {
        public ServerSettings Server { get; set; }
        public SecuritySettings Security { get; set; }
        public AdminSettings Admin { get; set; }
        public DatabaseSettings Database { get; set; }
        public ChannelSettings Channels { get; set; }
        public LoggingSettings Logging { get; set; }
    }

    public class ServerSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
    }

    public class SecuritySettings
    {
        public string Encryption_Key { get; set; }
        public int Max_Connections { get; set; }
    }

    public class AdminSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class DatabaseSettings
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class ChannelSettings
    {
        public string Default_Channel { get; set; }
        public int Max_Channels { get; set; }
    }

    public class LoggingSettings
    {
        public string Level { get; set; }
        public string File { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public bool CanCreateChannels { get; set; }
        public DateTime LastLogin { get; set; }
        public List<string> OwnedChannels { get; set; } = new List<string>();
    }

    public class Channel
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public List<string> Members { get; set; } = new List<string>();
        public bool IsPrivate { get; set; }
        public string Password { get; set; }
        public DateTime Created { get; set; }
    }

    public class ClientConnection
    {
        public TcpClient TcpClient { get; set; }
        public NetworkStream Stream { get; set; }
        public string Username { get; set; }
        public string CurrentChannel { get; set; }
        public bool IsAuthenticated { get; set; }
        public User User { get; set; }
    }

    public class Message
    {
        public string Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public string Channel { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    public class VoiceServer
    {
        private ServerConfig _config;
        private TcpListener _tcpListener;
        private UdpClient _udpClient;
        private List<ClientConnection> _clients = new List<ClientConnection>();
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private Dictionary<string, Channel> _channels = new Dictionary<string, Channel>();
        private bool _isRunning = false;
        private readonly object _lockObject = new object();

        public VoiceServer()
        {
            LoadConfig();
            InitializeDatabase();
            CreateDefaultChannel();
        }

        private void LoadConfig()
        {
            try
            {
                var yaml = File.ReadAllText("config.yml");
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                _config = deserializer.Deserialize<ServerConfig>(yaml);
                Console.WriteLine($"Config loaded successfully. Server: {_config.Server.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void InitializeDatabase()
        {
            // Initialize admin user
            var adminPasswordHash = HashPassword(_config.Admin.Password);
            _users[_config.Admin.Username] = new User
            {
                Username = _config.Admin.Username,
                PasswordHash = adminPasswordHash,
                IsAdmin = true,
                CanCreateChannels = true,
                LastLogin = DateTime.Now
            };
            
            Console.WriteLine($"Admin user '{_config.Admin.Username}' initialized");
        }

        private void CreateDefaultChannel()
        {
            _channels[_config.Channels.Default_Channel] = new Channel
            {
                Name = _config.Channels.Default_Channel,
                Owner = _config.Admin.Username,
                IsPrivate = false,
                Created = DateTime.Now
            };
            
            Console.WriteLine($"Default channel '{_config.Channels.Default_Channel}' created");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "EncryptItVC_Salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public async Task StartAsync()
        {
            _isRunning = true;
            
            // Start TCP listener for connections
            _tcpListener = new TcpListener(IPAddress.Parse(_config.Server.Host), _config.Server.Port);
            _tcpListener.Start();
            
            // Start UDP client for voice data
            _udpClient = new UdpClient(_config.Server.Port + 1);
            
            Console.WriteLine($"🎙️  EncryptItVC Server started on {_config.Server.Host}:{_config.Server.Port}");
            Console.WriteLine($"🔊 Voice server running on port {_config.Server.Port + 1}");
            Console.WriteLine($"👑 Admin: {_config.Admin.Username}");
            Console.WriteLine("Press Ctrl+C to stop the server");
            
            // Start accepting connections
            _ = Task.Run(AcceptClientsAsync);
            _ = Task.Run(HandleVoiceDataAsync);
            
            // Keep server running
            while (_isRunning)
            {
                await Task.Delay(1000);
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var connection = new ClientConnection
                    {
                        TcpClient = tcpClient,
                        Stream = tcpClient.GetStream(),
                        IsAuthenticated = false
                    };
                    
                    lock (_lockObject)
                    {
                        _clients.Add(connection);
                    }
                    
                    Console.WriteLine($"🔗 New client connected: {tcpClient.Client.RemoteEndPoint}");
                    
                    // Handle client in separate task
                    _ = Task.Run(() => HandleClientAsync(connection));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(ClientConnection connection)
        {
            var buffer = new byte[4096];
            
            try
            {
                while (connection.TcpClient.Connected && _isRunning)
                {
                    var bytesRead = await connection.Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    
                    var messageData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"📨 Received message: {messageData}");
                    
                    try
                    {
                        var message = JsonConvert.DeserializeObject<Message>(messageData);
                        if (message != null)
                        {
                            await ProcessMessageAsync(connection, message);
                        }
                        else
                        {
                            Console.WriteLine("❌ Failed to deserialize message");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ JSON parsing error: {ex.Message}");
                        Console.WriteLine($"   Raw data: {messageData}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                DisconnectClient(connection);
            }
        }

        private async Task ProcessMessageAsync(ClientConnection connection, Message message)
        {
            switch (message.Type)
            {
                case "LOGIN":
                    await HandleLoginAsync(connection, message);
                    break;
                case "REGISTER":
                    await HandleRegisterAsync(connection, message);
                    break;
                case "CREATE_CHANNEL":
                    await HandleCreateChannelAsync(connection, message);
                    break;
                case "JOIN_CHANNEL":
                    await HandleJoinChannelAsync(connection, message);
                    break;
                case "LEAVE_CHANNEL":
                    await HandleLeaveChannelAsync(connection, message);
                    break;
                case "CHAT_MESSAGE":
                    await HandleChatMessageAsync(connection, message);
                    break;
                case "GET_CHANNELS":
                    await HandleGetChannelsAsync(connection);
                    break;
                case "GET_USERS":
                    await HandleGetUsersAsync(connection, message);
                    break;
                case "GRANT_PERMISSION":
                    await HandleGrantPermissionAsync(connection, message);
                    break;
                default:
                    Console.WriteLine($"Unknown message type: {message.Type}");
                    break;
            }
        }

        private async Task HandleLoginAsync(ClientConnection connection, Message message)
        {
            var username = message.Data["username"].ToString();
            var password = message.Data["password"].ToString();
            
            if (_users.ContainsKey(username) && VerifyPassword(password, _users[username].PasswordHash))
            {
                connection.Username = username;
                connection.User = _users[username];
                connection.IsAuthenticated = true;
                connection.CurrentChannel = _config.Channels.Default_Channel;
                
                // Add user to default channel
                lock (_lockObject)
                {
                    if (!_channels[_config.Channels.Default_Channel].Members.Contains(username))
                    {
                        _channels[_config.Channels.Default_Channel].Members.Add(username);
                    }
                }
                
                var response = new Message
                {
                    Type = "LOGIN_SUCCESS",
                    Data = new Dictionary<string, object>
                    {
                        ["username"] = username,
                        ["isAdmin"] = _users[username].IsAdmin,
                        ["canCreateChannels"] = _users[username].CanCreateChannels,
                        ["currentChannel"] = _config.Channels.Default_Channel
                    }
                };
                
                await SendMessageAsync(connection, response);
                Console.WriteLine($"✅ User '{username}' logged in successfully");
                
                // Broadcast user joined
                await BroadcastToChannelAsync(_config.Channels.Default_Channel, new Message
                {
                    Type = "USER_JOINED",
                    From = "SERVER",
                    Channel = _config.Channels.Default_Channel,
                    Content = $"{username} joined the channel"
                }, connection);
            }
            else
            {
                var response = new Message
                {
                    Type = "LOGIN_FAILED",
                    Content = "Invalid username or password"
                };
                await SendMessageAsync(connection, response);
                Console.WriteLine($"❌ Failed login attempt for '{username}'");
            }
        }

        private async Task HandleRegisterAsync(ClientConnection connection, Message message)
        {
            var username = message.Data["username"].ToString();
            var password = message.Data["password"].ToString();
            
            if (_users.ContainsKey(username))
            {
                var response = new Message
                {
                    Type = "REGISTER_FAILED",
                    Content = "Username already exists"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                IsAdmin = false,
                CanCreateChannels = false,
                LastLogin = DateTime.Now
            };
            
            _users[username] = newUser;
            
            var response2 = new Message
            {
                Type = "REGISTER_SUCCESS",
                Content = "User registered successfully"
            };
            
            await SendMessageAsync(connection, response2);
            Console.WriteLine($"📝 New user '{username}' registered");
        }

        private async Task HandleCreateChannelAsync(ClientConnection connection, Message message)
        {
            if (!connection.IsAuthenticated || !connection.User.CanCreateChannels)
            {
                var response = new Message
                {
                    Type = "ERROR",
                    Content = "No permission to create channels"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            var channelName = message.Data["channelName"].ToString();
            var isPrivate = (bool)message.Data["isPrivate"];
            var password = message.Data.ContainsKey("password") ? message.Data["password"].ToString() : "";
            
            if (_channels.ContainsKey(channelName))
            {
                var response = new Message
                {
                    Type = "ERROR",
                    Content = "Channel already exists"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            var newChannel = new Channel
            {
                Name = channelName,
                Owner = connection.Username,
                IsPrivate = isPrivate,
                Password = password,
                Created = DateTime.Now
            };
            
            _channels[channelName] = newChannel;
            connection.User.OwnedChannels.Add(channelName);
            
            var response2 = new Message
            {
                Type = "CHANNEL_CREATED",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = channelName,
                    ["owner"] = connection.Username,
                    ["isPrivate"] = isPrivate
                }
            };
            
            await SendMessageAsync(connection, response2);
            Console.WriteLine($"🎯 Channel '{channelName}' created by '{connection.Username}'");
        }

        private async Task HandleJoinChannelAsync(ClientConnection connection, Message message)
        {
            if (!connection.IsAuthenticated) return;
            
            var channelName = message.Data["channelName"].ToString();
            var password = message.Data.ContainsKey("password") ? message.Data["password"].ToString() : "";
            
            if (!_channels.ContainsKey(channelName))
            {
                var response = new Message
                {
                    Type = "ERROR",
                    Content = "Channel does not exist"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            var channel = _channels[channelName];
            
            // Check if private channel and user has permission
            if (channel.IsPrivate && !string.IsNullOrEmpty(channel.Password) && channel.Password != password)
            {
                var response = new Message
                {
                    Type = "ERROR",
                    Content = "Incorrect channel password"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            // Leave current channel
            if (!string.IsNullOrEmpty(connection.CurrentChannel))
            {
                lock (_lockObject)
                {
                    _channels[connection.CurrentChannel].Members.Remove(connection.Username);
                }
                
                await BroadcastToChannelAsync(connection.CurrentChannel, new Message
                {
                    Type = "USER_LEFT",
                    From = "SERVER",
                    Channel = connection.CurrentChannel,
                    Content = $"{connection.Username} left the channel"
                }, connection);
            }
            
            // Join new channel
            connection.CurrentChannel = channelName;
            lock (_lockObject)
            {
                if (!channel.Members.Contains(connection.Username))
                {
                    channel.Members.Add(connection.Username);
                }
            }
            
            var response2 = new Message
            {
                Type = "CHANNEL_JOINED",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = channelName,
                    ["members"] = channel.Members
                }
            };
            
            await SendMessageAsync(connection, response2);
            
            // Broadcast user joined
            await BroadcastToChannelAsync(channelName, new Message
            {
                Type = "USER_JOINED",
                From = "SERVER",
                Channel = channelName,
                Content = $"{connection.Username} joined the channel"
            }, connection);
            
            Console.WriteLine($"👥 User '{connection.Username}' joined channel '{channelName}'");
        }

        private async Task HandleLeaveChannelAsync(ClientConnection connection, Message message)
        {
            if (!connection.IsAuthenticated || string.IsNullOrEmpty(connection.CurrentChannel)) return;
            
            var currentChannel = connection.CurrentChannel;
            
            lock (_lockObject)
            {
                _channels[currentChannel].Members.Remove(connection.Username);
            }
            
            connection.CurrentChannel = _config.Channels.Default_Channel;
            
            lock (_lockObject)
            {
                if (!_channels[_config.Channels.Default_Channel].Members.Contains(connection.Username))
                {
                    _channels[_config.Channels.Default_Channel].Members.Add(connection.Username);
                }
            }
            
            await BroadcastToChannelAsync(currentChannel, new Message
            {
                Type = "USER_LEFT",
                From = "SERVER",
                Channel = currentChannel,
                Content = $"{connection.Username} left the channel"
            }, connection);
            
            var response = new Message
            {
                Type = "CHANNEL_LEFT",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = _config.Channels.Default_Channel
                }
            };
            
            await SendMessageAsync(connection, response);
        }

        private async Task HandleChatMessageAsync(ClientConnection connection, Message message)
        {
            if (!connection.IsAuthenticated) return;
            
            var chatMessage = new Message
            {
                Type = "CHAT_MESSAGE",
                From = connection.Username,
                Channel = connection.CurrentChannel,
                Content = message.Content,
                Timestamp = DateTime.Now
            };
            
            await BroadcastToChannelAsync(connection.CurrentChannel, chatMessage, connection);
            Console.WriteLine($"💬 [{connection.CurrentChannel}] {connection.Username}: {message.Content}");
        }

        private async Task HandleGetChannelsAsync(ClientConnection connection)
        {
            if (!connection.IsAuthenticated) return;
            
            var channelList = new List<object>();
            
            lock (_lockObject)
            {
                foreach (var channel in _channels)
                {
                    channelList.Add(new
                    {
                        name = channel.Key,
                        owner = channel.Value.Owner,
                        memberCount = channel.Value.Members.Count,
                        isPrivate = channel.Value.IsPrivate,
                        hasPassword = !string.IsNullOrEmpty(channel.Value.Password)
                    });
                }
            }
            
            var response = new Message
            {
                Type = "CHANNELS_LIST",
                Data = new Dictionary<string, object>
                {
                    ["channels"] = channelList
                }
            };
            
            await SendMessageAsync(connection, response);
        }

        private async Task HandleGetUsersAsync(ClientConnection connection, Message message)
        {
            if (!connection.IsAuthenticated) return;
            
            var channelName = message.Data["channelName"].ToString();
            
            if (!_channels.ContainsKey(channelName))
            {
                var response = new Message
                {
                    Type = "ERROR",
                    Content = "Channel does not exist"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            var response2 = new Message
            {
                Type = "USERS_LIST",
                Data = new Dictionary<string, object>
                {
                    ["channelName"] = channelName,
                    ["users"] = _channels[channelName].Members
                }
            };
            
            await SendMessageAsync(connection, response2);
        }

        private async Task HandleGrantPermissionAsync(ClientConnection connection, Message message)
        {
            if (!connection.IsAuthenticated || !connection.User.IsAdmin) return;
            
            var targetUsername = message.Data["username"].ToString();
            var permission = message.Data["permission"].ToString();
            
            if (!_users.ContainsKey(targetUsername))
            {
                var response = new Message
                {
                    Type = "ERROR",
                    Content = "User does not exist"
                };
                await SendMessageAsync(connection, response);
                return;
            }
            
            switch (permission)
            {
                case "CREATE_CHANNELS":
                    _users[targetUsername].CanCreateChannels = true;
                    break;
                case "ADMIN":
                    _users[targetUsername].IsAdmin = true;
                    _users[targetUsername].CanCreateChannels = true;
                    break;
            }
            
            var response2 = new Message
            {
                Type = "PERMISSION_GRANTED",
                Data = new Dictionary<string, object>
                {
                    ["username"] = targetUsername,
                    ["permission"] = permission
                }
            };
            
            await SendMessageAsync(connection, response2);
            Console.WriteLine($"🔑 Permission '{permission}' granted to '{targetUsername}' by '{connection.Username}'");
        }

        private async Task HandleVoiceDataAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    var voiceData = result.Buffer;
                    var endpoint = result.RemoteEndPoint;
                    
                    // Find the client by endpoint and broadcast voice data to same channel
                    // This is a simplified implementation
                    await BroadcastVoiceDataAsync(voiceData, endpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Voice data error: {ex.Message}");
                }
            }
        }

        private async Task BroadcastVoiceDataAsync(byte[] voiceData, IPEndPoint senderEndpoint)
        {
            try
            {
                // Find the sender client by endpoint
                ClientConnection? senderClient = null;
                foreach (var client in _clients)
                {
                    // Match by IP address (simplified approach)
                    if (client.TcpClient.Client.RemoteEndPoint is IPEndPoint clientEndpoint &&
                        clientEndpoint.Address.Equals(senderEndpoint.Address))
                    {
                        senderClient = client;
                        break;
                    }
                }

                if (senderClient == null)
                {
                    Console.WriteLine($"Could not find sender client for voice data from {senderEndpoint}");
                    return;
                }

                // Get the sender's channel
                var senderChannel = senderClient.CurrentChannel;
                if (string.IsNullOrEmpty(senderChannel))
                {
                    Console.WriteLine($"Sender {senderClient.Username} not in any channel");
                    return;
                }

                // Broadcast to all clients in the same channel (except sender)
                var channelClients = _clients.Where(c => 
                    c.CurrentChannel == senderChannel && 
                    c.Username != senderClient.Username).ToList();

                if (channelClients.Any())
                {
                    Console.WriteLine($"Broadcasting voice data from {senderClient.Username} to {channelClients.Count} clients in channel {senderChannel}");
                    
                    // Send voice data to each client via UDP
                    foreach (var client in channelClients)
                    {
                        try
                        {
                            if (client.TcpClient.Client.RemoteEndPoint is IPEndPoint clientEndpoint)
                            {
                                // Send to client UDP port (same as TCP port + 1, but we'll use a different approach)
                                // For now, send to the same port the voice data came from
                                await _udpClient.SendAsync(voiceData, voiceData.Length, clientEndpoint.Address.ToString(), senderEndpoint.Port);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending voice data to {client.Username}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BroadcastVoiceDataAsync: {ex.Message}");
            }
        }

        private async Task SendMessageAsync(ClientConnection connection, Message message)
        {
            try
            {
                var json = JsonConvert.SerializeObject(message);
                var data = Encoding.UTF8.GetBytes(json);
                await connection.Stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private async Task BroadcastToChannelAsync(string channelName, Message message, ClientConnection excludeConnection = null)
        {
            if (!_channels.ContainsKey(channelName)) return;
            
            var channel = _channels[channelName];
            var tasks = new List<Task>();
            
            lock (_lockObject)
            {
                foreach (var client in _clients)
                {
                    if (client.CurrentChannel == channelName && 
                        client.IsAuthenticated && 
                        client != excludeConnection)
                    {
                        tasks.Add(SendMessageAsync(client, message));
                    }
                }
            }
            
            await Task.WhenAll(tasks);
        }

        private void DisconnectClient(ClientConnection connection)
        {
            try
            {
                if (!string.IsNullOrEmpty(connection.CurrentChannel))
                {
                    lock (_lockObject)
                    {
                        _channels[connection.CurrentChannel].Members.Remove(connection.Username);
                    }
                    
                    _ = BroadcastToChannelAsync(connection.CurrentChannel, new Message
                    {
                        Type = "USER_LEFT",
                        From = "SERVER",
                        Channel = connection.CurrentChannel,
                        Content = $"{connection.Username} disconnected"
                    }, connection);
                }
                
                connection.TcpClient?.Close();
                
                lock (_lockObject)
                {
                    _clients.Remove(connection);
                }
                
                Console.WriteLine($"🔌 Client '{connection.Username}' disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting client: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _tcpListener?.Stop();
            _udpClient?.Close();
            
            lock (_lockObject)
            {
                foreach (var client in _clients)
                {
                    client.TcpClient?.Close();
                }
                _clients.Clear();
            }
            
            Console.WriteLine("🛑 Server stopped");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "EncryptItVC Server";
            Console.WriteLine("🎙️  EncryptItVC Server");
            Console.WriteLine("========================");
            
            var server = new VoiceServer();
            
            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                server.Stop();
                Environment.Exit(0);
            };
            
            await server.StartAsync();
        }
    }
}
