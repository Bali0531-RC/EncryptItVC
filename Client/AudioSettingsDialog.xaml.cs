using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Threading.Tasks;
using System.Windows.Threading;
using EncryptItVC.Client.Models;

namespace EncryptItVC.Client
{
    public partial class AudioSettingsDialog : Window
    {
        private VoiceManager voiceManager;
        private WaveIn testWaveIn;
        private WaveOut testWaveOut;
        private DispatcherTimer inputLevelTimer;
        private bool isTestingInput = false;
        private bool isTestingOutput = false;
        private bool isMonitoring = false;
        private BufferedWaveProvider? monitorWaveProvider;

        public int SelectedInputDevice { get; private set; } = -1;
        public int SelectedOutputDevice { get; private set; } = -1;
        public int VolumeLevel { get; private set; } = 100;
        public bool EchoCancellation { get; private set; } = true;
        public bool NoiseSuppression { get; private set; } = true;

        public AudioSettingsDialog(VoiceManager voiceManager = null)
        {
            InitializeComponent();
            this.voiceManager = voiceManager;
            LoadAudioDevices();
            LoadCurrentSettings();
            
            inputLevelTimer = new DispatcherTimer();
            inputLevelTimer.Interval = TimeSpan.FromMilliseconds(100);
            inputLevelTimer.Tick += InputLevelTimer_Tick;
        }

        private void LoadAudioDevices()
        {
            try
            {
                // Load input devices
                var inputDevices = new List<string>();
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var deviceInfo = WaveIn.GetCapabilities(i);
                    inputDevices.Add($"🎤 {deviceInfo.ProductName}");
                }
                InputDeviceComboBox.ItemsSource = inputDevices;
                if (inputDevices.Count > 0)
                    InputDeviceComboBox.SelectedIndex = 0;

                // Load output devices
                var outputDevices = new List<string>();
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                {
                    var deviceInfo = WaveOut.GetCapabilities(i);
                    outputDevices.Add($"🔊 {deviceInfo.ProductName}");
                }
                OutputDeviceComboBox.ItemsSource = outputDevices;
                if (outputDevices.Count > 0)
                    OutputDeviceComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audio devices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadCurrentSettings()
        {
            try
            {
                if (voiceManager != null)
                {
                    var inputDevices = voiceManager.GetInputDevices();
                    var outputDevices = voiceManager.GetOutputDevices();
                    
                    // Set currently selected devices if available
                    if (inputDevices.Count > 0 && InputDeviceComboBox.Items.Count > 0)
                    {
                        InputDeviceComboBox.SelectedIndex = 0;
                        SelectedInputDevice = 0;
                    }
                    
                    if (outputDevices.Count > 0 && OutputDeviceComboBox.Items.Count > 0)
                    {
                        OutputDeviceComboBox.SelectedIndex = 0;
                        SelectedOutputDevice = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading current settings: {ex.Message}");
            }
        }

        private void TestInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (isTestingInput)
            {
                StopInputTest();
            }
            else
            {
                StartInputTest();
            }
        }

        private void StartInputTest()
        {
            try
            {
                if (InputDeviceComboBox.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select an input device first.", "No Device Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                testWaveIn = new WaveIn();
                testWaveIn.DeviceNumber = InputDeviceComboBox.SelectedIndex;
                testWaveIn.WaveFormat = new WaveFormat(44100, 16, 1);
                testWaveIn.DataAvailable += TestWaveIn_DataAvailable;
                testWaveIn.StartRecording();
                
                // Initialize monitor output for hearing yourself
                if (OutputDeviceComboBox.SelectedIndex >= 0)
                {
                    StartMonitoring();
                }
                
                isTestingInput = true;
                TestInputButton.Content = "⏹️ Stop Test";
                TestInputButton.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                
                inputLevelTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting input test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopInputTest()
        {
            try
            {
                if (testWaveIn != null)
                {
                    testWaveIn.StopRecording();
                    testWaveIn.Dispose();
                    testWaveIn = null;
                }
                
                StopMonitoring();
                
                isTestingInput = false;
                TestInputButton.Content = "🎤 Test Input";
                TestInputButton.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                
                inputLevelTimer.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping input test: {ex.Message}");
            }
        }

        private void TestWaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Play the captured audio for monitoring (hearing yourself)
            if (isMonitoring && monitorWaveProvider != null && e.BytesRecorded > 0)
            {
                monitorWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void InputLevelTimer_Tick(object sender, EventArgs e)
        {
            if (isTestingInput)
            {
                // Simple visual feedback - in a real implementation, you'd analyze the audio data
                var random = new Random();
                var level = random.Next(20, 100);
                
                // Change button color based on "audio level"
                if (level > 70)
                    TestInputButton.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                else if (level > 40)
                    TestInputButton.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow
                else
                    TestInputButton.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
            }
        }

        private void TestOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (isTestingOutput)
            {
                StopOutputTest();
            }
            else
            {
                StartOutputTest();
            }
        }

        private void StartOutputTest()
        {
            try
            {
                if (OutputDeviceComboBox.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select an output device first.", "No Device Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Generate a simple test tone
                var testTone = GenerateTestTone();
                
                testWaveOut = new WaveOut();
                testWaveOut.DeviceNumber = OutputDeviceComboBox.SelectedIndex;
                testWaveOut.Init(testTone);
                testWaveOut.Play();
                
                isTestingOutput = true;
                TestOutputButton.Content = "⏹️ Stop Test";
                TestOutputButton.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                
                // Auto-stop after 3 seconds
                Task.Delay(3000).ContinueWith(t => 
                {
                    Dispatcher.Invoke(() => StopOutputTest());
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting output test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopOutputTest()
        {
            try
            {
                if (testWaveOut != null)
                {
                    testWaveOut.Stop();
                    testWaveOut.Dispose();
                    testWaveOut = null;
                }
                
                isTestingOutput = false;
                TestOutputButton.Content = "🔊 Test Output";
                TestOutputButton.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping output test: {ex.Message}");
            }
        }

        private ISampleProvider GenerateTestTone()
        {
            var frequency = 440.0; // A4 note
            var amplitude = 0.3f;
            var sampleRate = 44100;
            
            return new TestToneGenerator(frequency, amplitude, sampleRate);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Stop any ongoing tests
                StopInputTest();
                StopOutputTest();
                
                // Save selected settings
                SelectedInputDevice = InputDeviceComboBox.SelectedIndex;
                SelectedOutputDevice = OutputDeviceComboBox.SelectedIndex;
                VolumeLevel = (int)VolumeSlider.Value;
                EchoCancellation = EnableEchoCancellationCheckBox.IsChecked ?? false;
                NoiseSuppression = EnableNoiseSuppression.IsChecked ?? false;
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            StopInputTest();
            StopOutputTest();
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                StopInputTest();
                StopOutputTest();
                
                if (inputLevelTimer != null)
                {
                    inputLevelTimer.Stop();
                    inputLevelTimer = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
            
            base.OnClosed(e);
        }

        private void StartMonitoring()
        {
            try
            {
                if (OutputDeviceComboBox.SelectedIndex < 0) return;

                var waveFormat = new WaveFormat(44100, 16, 1);
                monitorWaveProvider = new BufferedWaveProvider(waveFormat);
                monitorWaveProvider.BufferLength = waveFormat.SampleRate * 2; // 2 seconds buffer
                monitorWaveProvider.DiscardOnBufferOverflow = true;
                
                testWaveOut = new WaveOut();
                testWaveOut.DeviceNumber = OutputDeviceComboBox.SelectedIndex;
                testWaveOut.Init(monitorWaveProvider);
                testWaveOut.Volume = (float)(VolumeSlider.Value / 100.0);
                testWaveOut.Play();
                
                isMonitoring = true;
                Console.WriteLine("Monitoring started - you should hear yourself");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting monitoring: {ex.Message}");
                MessageBox.Show($"Could not start audio monitoring: {ex.Message}", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StopMonitoring()
        {
            try
            {
                if (isMonitoring)
                {
                    testWaveOut?.Stop();
                    testWaveOut?.Dispose();
                    testWaveOut = null;
                    
                    monitorWaveProvider = null;
                    isMonitoring = false;
                    Console.WriteLine("Monitoring stopped");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping monitoring: {ex.Message}");
            }
        }
    }

    // Simple test tone generator for audio output testing
    public class TestToneGenerator : ISampleProvider
    {
        private readonly double frequency;
        private readonly float amplitude;
        private double phase;
        private readonly WaveFormat waveFormat;

        public TestToneGenerator(double frequency, float amplitude, int sampleRate)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        }

        public WaveFormat WaveFormat => waveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = amplitude * (float)Math.Sin(phase);
                phase += 2 * Math.PI * frequency / waveFormat.SampleRate;
                if (phase > 2 * Math.PI)
                    phase -= 2 * Math.PI;
            }
            return count;
        }
    }
}
