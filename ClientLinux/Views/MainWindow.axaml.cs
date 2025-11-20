using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EncryptItVC.ClientLinux.Models;
using EncryptItVC.ClientLinux.ViewModels;

namespace EncryptItVC.ClientLinux.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        public MainWindow(ServerConnection serverConnection)
        {
            InitializeComponent();
            DataContext = new MainViewModel(serverConnection);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
