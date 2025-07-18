using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;
using NAudio.Wave;
using System.Net;
using System.Threading;

namespace EncryptItVC.Client.Models
{
    public class ServerConnection : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;
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
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                _isAuthenticated = value;
                OnPropertyChanged();
            }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                _isAdmin = value;
                OnPropertyChanged();
            }
        }

        public bool CanCreateChannels
        {
            get => _canCreateChannels;
            set
            {
                _canCreateChannels = value;
                OnPropertyChanged();
            }
        }

        public string CurrentChannel
        {
            get => _currentChannel;
            set
            {
                _currentChannel = value;
                OnPropertyChanged();
            }
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                // Store server connection details
                _serverHost = host;
                _serverPort = port;
                
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _stream = _tcpClient.GetStream();
                
                // Connect UDP for voice
                _udpClient = new UdpClient();
                _udpClient.Connect(host, port + 1);
                
                IsConnected = true;
                
                // Start receiving messages
                _ = Task.Run(ReceiveMessagesAsync);
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            
            // Store password for potential reconnection
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

        public async Task<bool> GrantPermissionAsync(string username, string permission)
        {
            var message = new Message
            {
                Type = "GRANT_PERMISSION",
                Data = new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["permission"] = permission
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
                    MessageBox.Show("Not connected to server", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show($"Send message failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Ha a kapcsolat megszakadt, jelöljük úgy
                if (!_tcpClient?.Connected == true)
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
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                MessageReceived?.Invoke(message);
                            });
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
                    // Notify UI about connection loss
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
                MessageBox.Show($"Disconnect failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }

    public class VoiceManager
    {
        private WaveInEvent? _waveIn;
        private WaveOutEvent? _waveOut;
        private UdpClient? _udpClient;
        private bool _isRecording;
        private bool _isPlaying;
        private Task? _udpReceiveTask;
        private int _inputDeviceIndex = -1;
        private int _outputDeviceIndex = -1;
        private CancellationTokenSource _cancellationTokenSource = new();
        private BufferedWaveProvider? _waveProvider;

        public bool IsRecording
        {
            get => _isRecording;
            set => _isRecording = value;
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => _isPlaying = value;
        }

        public List<string> GetInputDevices()
        {
            var devices = new List<string>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                devices.Add($"{i}: {capabilities.ProductName}");
            }
            return devices;
        }

        public List<string> GetOutputDevices()
        {
            var devices = new List<string>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                devices.Add($"{i}: {capabilities.ProductName}");
            }
            return devices;
        }

        public void SetInputDevice(int deviceIndex)
        {
            _inputDeviceIndex = deviceIndex;
        }

        public void SetOutputDevice(int deviceIndex)
        {
            _outputDeviceIndex = deviceIndex;
        }

        public void SetVolume(int volumeLevel)
        {
            if (_waveOut != null)
            {
                _waveOut.Volume = volumeLevel / 100.0f;
            }
        }

        public void StartRecording(UdpClient? udpClient)
        {
            if (_isRecording) return;
            
            try
            {
                _udpClient = udpClient;
                _waveIn = new WaveInEvent();
                
                // Set specific input device if selected
                if (_inputDeviceIndex >= 0 && _inputDeviceIndex < WaveIn.DeviceCount)
                {
                    _waveIn.DeviceNumber = _inputDeviceIndex;
                }
                
                _waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.StartRecording();
                _isRecording = true;
                
                // Initialize audio playback
                InitializeAudioPlayback();
                
                // Start UDP receive task for voice data
                if (_udpClient != null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _udpReceiveTask = Task.Run(() => ReceiveVoiceDataAsync(_cancellationTokenSource.Token));
                }
                
                Console.WriteLine("Voice recording started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start recording: {ex.Message}");
                StopRecording();
            }
        }

        private void InitializeAudioPlayback()
        {
            try
            {
                var waveFormat = new WaveFormat(44100, 16, 1);
                _waveProvider = new BufferedWaveProvider(waveFormat);
                _waveProvider.BufferLength = waveFormat.SampleRate * 2; // 2 seconds buffer
                _waveProvider.DiscardOnBufferOverflow = true;
                
                _waveOut = new WaveOutEvent();
                
                // Set specific output device if selected
                if (_outputDeviceIndex >= 0 && _outputDeviceIndex < WaveOut.DeviceCount)
                {
                    _waveOut.DeviceNumber = _outputDeviceIndex;
                }
                
                _waveOut.Init(_waveProvider);
                _waveOut.Play();
                _isPlaying = true;
                
                Console.WriteLine("Audio playback initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize audio playback: {ex.Message}");
            }
        }

        public void StopRecording()
        {
            if (!_isRecording) return;
            
            try
            {
                _isRecording = false;
                _isPlaying = false;
                
                _cancellationTokenSource?.Cancel();
                
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;
                
                _waveOut?.Stop();
                _waveOut?.Dispose();
                _waveOut = null;
                
                _waveProvider = null;
                
                Console.WriteLine("Voice recording stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping recording: {ex.Message}");
            }
        }

        private async void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_udpClient != null && _isRecording)
            {
                try
                {
                    // Add voice data identifier
                    var voicePacket = new byte[e.BytesRecorded + 4];
                    BitConverter.GetBytes(e.BytesRecorded).CopyTo(voicePacket, 0);
                    Array.Copy(e.Buffer, 0, voicePacket, 4, e.BytesRecorded);
                    
                    await _udpClient.SendAsync(voicePacket, voicePacket.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Voice send error: {ex.Message}");
                }
            }
        }

        private async Task ReceiveVoiceDataAsync(CancellationToken cancellationToken)
        {
            if (_udpClient == null) return;
            
            try
            {
                Console.WriteLine("Starting voice receive loop");
                
                while (!cancellationToken.IsCancellationRequested && _isRecording)
                {
                    try
                    {
                        var result = await _udpClient.ReceiveAsync();
                        if (result.Buffer.Length > 4) // At least 4 bytes for length + some data
                        {
                            var dataLength = BitConverter.ToInt32(result.Buffer, 0);
                            if (dataLength > 0 && dataLength <= result.Buffer.Length - 4)
                            {
                                var voiceData = new byte[dataLength];
                                Array.Copy(result.Buffer, 4, voiceData, 0, dataLength);
                                PlayVoiceData(voiceData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Console.WriteLine($"Voice receive error: {ex.Message}");
                            await Task.Delay(100, cancellationToken); // Brief delay before retry
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Voice receive task error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Voice receive loop ended");
            }
        }

        public void PlayVoiceData(byte[] voiceData)
        {
            try
            {
                if (_waveProvider != null && _isPlaying && voiceData.Length > 0)
                {
                    _waveProvider.AddSamples(voiceData, 0, voiceData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Voice playback error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopRecording();
            _cancellationTokenSource?.Dispose();
            _udpReceiveTask?.Wait(1000); // Wait up to 1 second for cleanup
        }
    }
}
