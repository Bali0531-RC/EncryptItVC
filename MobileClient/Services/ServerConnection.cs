using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EncryptItVC.MobileClient.Services;

public class ServerConnection
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private bool _isConnected = false;

    public event EventHandler<string>? MessageReceived;
    public event EventHandler<bool>? ConnectionStatusChanged;

    public bool IsConnected => _isConnected && _tcpClient?.Connected == true;

    public async Task<bool> ConnectAsync(string host, int port)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port);
            _stream = _tcpClient.GetStream();
            _isConnected = true;

            ConnectionStatusChanged?.Invoke(this, true);

            // Start listening for messages
            _ = Task.Run(ListenForMessages);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        _isConnected = false;
        _stream?.Close();
        _tcpClient?.Close();
        ConnectionStatusChanged?.Invoke(this, false);
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        if (!IsConnected) return false;

        var loginMessage = new
        {
            type = "login",
            username = username,
            password = password
        };

        return await SendMessageAsync(JsonConvert.SerializeObject(loginMessage));
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        if (!IsConnected) return false;

        var registerMessage = new
        {
            type = "register",
            username = username,
            password = password
        };

        return await SendMessageAsync(JsonConvert.SerializeObject(registerMessage));
    }

    public async Task<bool> JoinChannelAsync(string channelName, string? password = null)
    {
        if (!IsConnected) return false;

        var joinMessage = new
        {
            type = "join_channel",
            channel = channelName,
            password = password
        };

        return await SendMessageAsync(JsonConvert.SerializeObject(joinMessage));
    }

    public async Task<bool> SendChatMessageAsync(string message)
    {
        if (!IsConnected) return false;

        var chatMessage = new
        {
            type = "chat_message",
            message = message
        };

        return await SendMessageAsync(JsonConvert.SerializeObject(chatMessage));
    }

    private async Task<bool> SendMessageAsync(string message)
    {
        try
        {
            if (_stream == null) return false;

            var data = Encoding.UTF8.GetBytes(message + "\n");
            await _stream.WriteAsync(data, 0, data.Length);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
            return false;
        }
    }

    private async Task ListenForMessages()
    {
        var buffer = new byte[4096];
        var messageBuilder = new StringBuilder();

        try
        {
            while (_isConnected && _stream != null)
            {
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(data);

                string messages = messageBuilder.ToString();
                string[] lines = messages.Split('\n');

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        ProcessMessage(lines[i]);
                    }
                }

                messageBuilder.Clear();
                if (lines.Length > 0 && !string.IsNullOrEmpty(lines[lines.Length - 1]))
                {
                    messageBuilder.Append(lines[lines.Length - 1]);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Listen error: {ex.Message}");
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            MessageReceived?.Invoke(this, message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Message processing error: {ex.Message}");
        }
    }
}
