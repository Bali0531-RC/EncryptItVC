using EncryptItVC.MobileClient.Views;

namespace EncryptItVC.MobileClient;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
