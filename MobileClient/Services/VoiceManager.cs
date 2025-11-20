using System.Net;
using System.Net.Sockets;

namespace EncryptItVC.MobileClient.Services;

public class VoiceManager
{
    private UdpClient? _udpClient;
    private IPEndPoint? _serverEndPoint;
    private bool _isMuted = false;
    private bool _isDeafened = false;
    private CancellationTokenSource? _cancellationTokenSource;

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            MuteStatusChanged?.Invoke(this, _isMuted);
        }
    }

    public bool IsDeafened
    {
        get => _isDeafened;
        set
        {
            _isDeafened = value;
            if (_isDeafened)
            {
                IsMuted = true; // Deafening also mutes
            }
            DeafenStatusChanged?.Invoke(this, _isDeafened);
        }
    }

    public event EventHandler<bool>? MuteStatusChanged;
    public event EventHandler<bool>? DeafenStatusChanged;

    public async Task InitializeAsync(string serverIp, int voicePort)
    {
        try
        {
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), voicePort);
            _udpClient = new UdpClient();
            _udpClient.Connect(_serverEndPoint);

            _cancellationTokenSource = new CancellationTokenSource();
            
            // Start listening for incoming voice data
            _ = Task.Run(async () => await ListenForVoiceData(_cancellationTokenSource.Token));
            
            System.Diagnostics.Debug.WriteLine($"Voice initialized - Server: {serverIp}:{voicePort}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Voice initialization error: {ex.Message}");
        }
    }

    private async Task ListenForVoiceData(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                var result = await _udpClient.ReceiveAsync();
                if (!_isDeafened && result.Buffer.Length > 0)
                {
                    // Voice data received - on mobile we would play it through the audio system
                    // For now, just log that we received it
                    System.Diagnostics.Debug.WriteLine($"Received voice data: {result.Buffer.Length} bytes");
                }
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine($"Voice receive error: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _udpClient?.Close();
        _udpClient?.Dispose();
    }
}
