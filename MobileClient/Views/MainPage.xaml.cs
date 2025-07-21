using EncryptItVC.MobileClient.ViewModels;

namespace EncryptItVC.MobileClient.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
