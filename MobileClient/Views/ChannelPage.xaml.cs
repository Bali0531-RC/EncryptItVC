using EncryptItVC.MobileClient.ViewModels;

namespace EncryptItVC.MobileClient.Views;

public partial class ChannelPage : ContentPage
{
	public ChannelPage(ChannelViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
