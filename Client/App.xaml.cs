using System.Windows;
using EncryptItVC.Client.Models;

namespace EncryptItVC.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Only create one LoginWindow instance
            var loginWindow = new LoginWindow();
            this.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}
