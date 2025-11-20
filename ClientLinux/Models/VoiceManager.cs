using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace EncryptItVC.ClientLinux.Models
{
    public class VoiceManager : IDisposable
    {
        private WaveInEvent? _waveIn;
        private WaveOutEvent? _waveOut;
        private UdpClient? _udpClient;
        private bool _isRecording;
        private bool _isPlaying;
        private CancellationTokenSource _cancellationTokenSource = new();
        private BufferedWaveProvider? _waveProvider;
        private bool _isMuted = false;
        private bool _isDeafened = false;
        private int _inputDeviceIndex = -1;
        private int _outputDeviceIndex = -1;
        
        public event Action<bool, bool>? StatusChanged;
        
        private ServerConnection? _serverConnection;
        
        public VoiceManager(ServerConnection? serverConnection = null)
        {
            _serverConnection = serverConnection;
        }

        public bool IsRecording => _isRecording;
        public bool IsPlaying => _isPlaying;
        
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                StatusChanged?.Invoke(_isMuted, _isDeafened);
                _ = Task.Run(() => _serverConnection?.UpdateVoiceStatusAsync(_isMuted, _isDeafened));
            }
        }
        
        public bool IsDeafened
        {
            get => _isDeafened;
            set
            {
                _isDeafened = value;
                if (_isDeafened) _isMuted = true;
                StatusChanged?.Invoke(_isMuted, _isDeafened);
                _ = Task.Run(() => _serverConnection?.UpdateVoiceStatusAsync(_isMuted, _isDeafened));
            }
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
        
        public void ToggleMute()
        {
            IsMuted = !IsMuted;
        }
        
        public void ToggleDeafen()
        {
            IsDeafened = !IsDeafened;
        }

        public void StartRecording(UdpClient? udpClient)
        {
            if (_isRecording) return;
            
            try
            {
                _udpClient = udpClient;
                _waveIn = new WaveInEvent();
                
                if (_inputDeviceIndex >= 0)
                {
                    try
                    {
                        _waveIn.DeviceNumber = _inputDeviceIndex;
                    }
                    catch
                    {
                        // Use default device
                    }
                }
                
                _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
                _waveIn.BufferMilliseconds = 50;
                _waveIn.NumberOfBuffers = 3;
                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.StartRecording();
                _isRecording = true;
                
                InitializeAudioPlayback();
                
                if (_udpClient != null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _ = Task.Run(() => ReceiveVoiceDataAsync(_cancellationTokenSource.Token));
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
                var waveFormat = new WaveFormat(16000, 16, 1);
                _waveProvider = new BufferedWaveProvider(waveFormat);
                _waveProvider.BufferLength = waveFormat.SampleRate * 1;
                _waveProvider.DiscardOnBufferOverflow = true;
                
                _waveOut = new WaveOutEvent();
                _waveOut.DesiredLatency = 100;
                
                if (_outputDeviceIndex >= 0)
                {
                    try
                    {
                        _waveOut.DeviceNumber = _outputDeviceIndex;
                    }
                    catch
                    {
                        // Use default device
                    }
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
            if (_udpClient != null && _isRecording && !_isMuted)
            {
                try
                {
                    await _udpClient.SendAsync(e.Buffer, e.BytesRecorded);
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
                        if (result.Buffer.Length > 0)
                        {
                            if (!_isDeafened)
                            {
                                PlayVoiceData(result.Buffer);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Console.WriteLine($"Voice receive error: {ex.Message}");
                            await Task.Delay(100, cancellationToken);
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
        }
    }
}
