using Plugin.Maui.Audio;
using System.Net;
using System.Net.Sockets;

namespace EncryptItVC.MobileClient.Services;

public class VoiceManager
{
    private readonly IAudioManager _audioManager;
    private IAudioRecorder? _audioRecorder;
    private IAudioPlayer? _audioPlayer;
    private UdpClient? _udpClient;
    private IPEndPoint? _serverEndPoint;
    private bool _isRecording = false;
    private bool _isMuted = false;
    private bool _isDeafened = false;
    private CancellationTokenSource? _cancellationTokenSource;

    public VoiceManager(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            if (_isMuted)
            {
                StopRecording();
            }
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
                _audioPlayer?.Stop();
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Voice initialization error: {ex.Message}");
        }
    }

    public async Task StartRecording()
    {
        if (_isMuted || _isRecording) return;

        try
        {
            _audioRecorder = _audioManager.CreateRecorder();
            await _audioRecorder.StartAsync("recording.wav");
            _isRecording = true;

            // Start sending voice data
            _ = Task.Run(SendVoiceData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Recording start error: {ex.Message}");
        }
    }

    public async Task StopRecording()
    {
        if (!_isRecording) return;

        try
        {
            _isRecording = false;
            if (_audioRecorder != null)
            {
                await _audioRecorder.StopAsync();
                _audioRecorder = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Recording stop error: {ex.Message}");
        }
    }

    private async Task SendVoiceData()
    {
        try
        {
            while (_isRecording && !_isMuted && _audioRecorder != null)
            {
                // For now, just send empty data as placeholder
                // In a full implementation, we would get audio data from the recorder
                var buffer = new byte[1024]; // Placeholder buffer

                // Send to server via UDP
                if (_udpClient != null && _serverEndPoint != null)
                {
                    await _udpClient.SendAsync(buffer, buffer.Length);
                }

                await Task.Delay(50); // Send chunks every 50ms
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Voice send error: {ex.Message}");
        }
    }

    private async Task ListenForVoiceData(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                var result = await _udpClient.ReceiveAsync();
                if (!_isDeafened)
                {
                    // Play received audio data
                    await PlayReceivedAudio(result.Buffer);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Voice receive error: {ex.Message}");
        }
    }

    private async Task PlayReceivedAudio(byte[] audioData)
    {
        try
        {
            // For now, just a placeholder
            // In a full implementation, we would convert the audio data and play it
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio playback error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _audioRecorder = null;
        _audioPlayer?.Stop();
        _audioPlayer = null;
        _udpClient?.Close();
        _udpClient?.Dispose();
    }
}
