using EncryptItVC.MobileClient.ViewModels;

namespace EncryptItVC.MobileClient.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
