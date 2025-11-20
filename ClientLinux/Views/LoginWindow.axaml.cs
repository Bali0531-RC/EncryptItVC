using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EncryptItVC.ClientLinux.ViewModels;

namespace EncryptItVC.ClientLinux.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            
            var viewModel = new LoginViewModel();
            DataContext = viewModel;
            
            viewModel.LoginSuccessful += (serverConnection) =>
            {
                var mainWindow = new MainWindow(serverConnection);
                mainWindow.Show();
                this.Close();
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
