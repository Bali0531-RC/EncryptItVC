using EncryptItVC.MobileClient.ViewModels;

namespace EncryptItVC.MobileClient.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
